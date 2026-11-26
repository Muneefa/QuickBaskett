using Microsoft.Extensions.Logging;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Application.Payments;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;
using QuickBasket.Payments.Abstractions;
using QuickBasket.Payments.Models;

namespace QuickBasket.Application.Services;

public sealed class PaymentWebhookService(
    IPaymentRepository payments,
    IPaymentProvider provider,
    PaymentStateUpdater state,
    IUnitOfWork uow,
    ILogger<PaymentWebhookService> logger) : IPaymentWebhookService
{
    // Steps 8-9 of the workflow: verify the signature on the RAW body, record the event once, then update state.
    public async Task<WebhookResult> HandleAsync(string rawBody, string? signature, string? eventIdHeader, CancellationToken ct = default)
    {
        if (!provider.VerifyWebhookSignature(rawBody, signature))
        {
            logger.LogWarning("Rejected webhook with invalid signature");
            throw new UnauthorizedAppException("Invalid webhook signature.");
        }

        GatewayWebhookEvent ev;
        try { ev = provider.ParseWebhook(rawBody, eventIdHeader); }
        catch (InvalidWebhookException ex) { throw new ValidationException(ex.Message); }

        // Idempotency: (Provider, EventId) is unique. A redelivery of a finished event is acknowledged and ignored.
        var existing = await payments.GetWebhookEventAsync(provider.Name, ev.EventId, ct);
        if (existing is { Status: WebhookProcessingStatus.Processed or WebhookProcessingStatus.Ignored })
            return new WebhookResult(true, "duplicate");

        if (existing is null)
        {
            try
            {
                await payments.AddWebhookEventAsync(new PaymentWebhookEvent
                {
                    Provider = provider.Name, EventId = ev.EventId, EventType = ev.EventType,
                    PayloadJson = WebhookPayloadSanitizer.Sanitize(rawBody)
                }, ct);
                await uow.SaveChangesAsync(ct);
            }
            catch (DuplicateRecordException)
            {
                // A parallel delivery of the same event inserted it first; its handler owns processing.
                uow.ClearTracking();
                return new WebhookResult(true, "duplicate");
            }
        }

        var outcome = await ConcurrencyRetry.RunAsync(uow, token => ProcessAsync(ev, token), ct);
        return new WebhookResult(true, outcome);
    }

    private async Task<string> ProcessAsync(GatewayWebhookEvent ev, CancellationToken ct)
    {
        var record = await payments.GetWebhookEventAsync(provider.Name, ev.EventId, ct)
                     ?? throw new InvalidOperationException("Webhook event record is missing.");
        if (record.Status is WebhookProcessingStatus.Processed or WebhookProcessingStatus.Ignored) return "duplicate";

        string outcome;
        Payment? payment = null;

        switch (ev.EventType)
        {
            case "payment.captured":
            case "payment.authorized":
            case "payment.failed":
            case "qr_code.credited":
            {
                payment = await ResolvePaymentAsync(ev, ct);
                if (payment is null || ev.GatewayPaymentId is null) { outcome = "ignored"; break; }

                var status = ev.EventType switch
                {
                    "payment.captured" or "qr_code.credited" => GatewayPaymentStatus.Captured,
                    "payment.authorized" => GatewayPaymentStatus.Authorized,
                    _ => GatewayPaymentStatus.Failed
                };
                // An explicit status in the payload wins over the event name when the gateway provides one.
                status = ev.PaymentStatus ?? status;

                var gp = new GatewayPayment(ev.GatewayPaymentId, ev.GatewayOrderId ?? payment.GatewayOrderId,
                    ev.AmountInPaise ?? 0, ev.Currency ?? payment.Currency, status, ev.Method, ev.ErrorDescription);

                var result = await state.ApplyGatewayPaymentAsync(payment, gp, "Webhook", ct);
                outcome = result switch { ApplyResult.Applied => "processed", ApplyResult.NoChange => "no-change", _ => "mismatch" };
                break;
            }

            case "qr_code.closed":
            {
                payment = ev.GatewayQrId is null ? null : await payments.GetByGatewayQrIdAsync(ev.GatewayQrId, ct);
                outcome = payment is not null && await state.ExpireAsync(payment, "Webhook", ct) ? "processed" : "ignored";
                break;
            }

            case "refund.processed":
            case "refund.failed":
            {
                var refund = ev.GatewayRefundId is null ? null : await payments.GetRefundByGatewayIdAsync(ev.GatewayRefundId, ct);
                if (refund is null) { outcome = "ignored"; break; }

                payment = await payments.GetByIdAsync(refund.PaymentId, ct);
                var rs = ev.RefundStatus ?? (ev.EventType == "refund.processed" ? GatewayRefundStatus.Processed : GatewayRefundStatus.Failed);
                outcome = payment is not null && await state.ApplyRefundResultAsync(refund, payment, rs, "Webhook", ct) ? "processed" : "no-change";
                break;
            }

            default:
                outcome = "ignored"; // event types we do not act on are acknowledged so the gateway stops retrying
                break;
        }

        record.PaymentId = payment?.Id;
        record.ProcessedAtUtc = DateTime.UtcNow;
        record.Status = outcome switch
        {
            "ignored" => WebhookProcessingStatus.Ignored,
            "mismatch" => WebhookProcessingStatus.Failed,
            _ => WebhookProcessingStatus.Processed
        };
        if (outcome == "mismatch") record.ErrorMessage = "Amount or order did not match. Manual review required.";

        logger.LogInformation("Webhook {EventId} ({EventType}) -> {Outcome}", ev.EventId, ev.EventType, outcome);
        return outcome;
    }

    private async Task<Payment?> ResolvePaymentAsync(GatewayWebhookEvent ev, CancellationToken ct)
    {
        if (ev.GatewayPaymentId is not null && await payments.GetByGatewayPaymentIdAsync(ev.GatewayPaymentId, ct) is { } byPayment) return byPayment;
        if (ev.GatewayOrderId is not null && await payments.GetByGatewayOrderIdAsync(ev.GatewayOrderId, ct) is { } byOrder) return byOrder;
        if (ev.GatewayQrId is not null && await payments.GetByGatewayQrIdAsync(ev.GatewayQrId, ct) is { } byQr) return byQr;
        return null;
    }
}

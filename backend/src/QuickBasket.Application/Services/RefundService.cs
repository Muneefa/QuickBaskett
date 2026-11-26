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

public sealed class RefundService(
    IPaymentRepository payments,
    IPaymentProvider provider,
    PaymentStateUpdater state,
    IUnitOfWork uow,
    ILogger<RefundService> logger) : IRefundService
{
    public async Task<RefundDto> RequestAsync(
        Guid adminUserId, Guid paymentId, RefundRequest request, string? idempotencyKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Length > 100)
            throw new ValidationException("An Idempotency-Key header (max 100 characters) is required for refunds.");

        if (await payments.GetRefundByIdempotencyKeyAsync(idempotencyKey, ct) is { } replay)
        {
            if (replay.PaymentId != paymentId) throw new ConflictException("This idempotency key was already used for another payment.");
            return replay.ToDto();
        }

        // 1) Reserve the refund in the database first, so concurrent requests cannot over-refund.
        var refundId = await ConcurrencyRetry.RunAsync(uow, async token =>
        {
            var payment = await payments.GetByIdAsync(paymentId, token) ?? throw new NotFoundException("Payment not found.");
            if (payment.Status is not (PaymentStatus.Captured or PaymentStatus.PartiallyRefunded))
                throw new ConflictException($"A {payment.Status} payment cannot be refunded.");

            var reserved = payment.Refunds.Where(r => r.Status == RefundStatus.Pending).Sum(r => r.Amount);
            var refundable = payment.Amount - payment.RefundedAmount - reserved;
            var amount = request.Amount ?? refundable;
            if (amount <= 0 || amount > refundable)
                throw new ValidationException($"Refund amount must be between 0.01 and {refundable} {payment.Currency}.");

            var refund = new Refund
            {
                PaymentId = payment.Id, OrderId = payment.OrderId, Amount = amount, Currency = payment.Currency,
                Reason = request.Reason, IdempotencyKey = idempotencyKey, RequestedByUserId = adminUserId
            };
            payment.Refunds.Add(refund);
            payment.UpdatedAtUtc = DateTime.UtcNow; // touches the row so concurrent refunds conflict on RowVersion
            await state.AddTransactionAsync(payment, PaymentTransactionType.RefundRequested, payment.Status, null, "Api",
                $"Refund of {amount} {payment.Currency} requested.", token, amount);
            return refund.Id;
        }, ct);

        var created = (await payments.GetRefundAsync(refundId, ct))!;
        var parent = (await payments.GetByIdAsync(paymentId, ct))!;

        // 2) Cash on delivery has no gateway: the refund is settled manually and recorded as processed.
        if (parent.Provider == "COD")
            return await FinishAsync(created.Id, GatewayRefundStatus.Processed, null, null, ct);

        // 3) Call the gateway outside the DB transaction. The gateway-side idempotency key is our refund id.
        try
        {
            var gw = await provider.RefundAsync(new RefundGatewayRequest(
                parent.GatewayPaymentId ?? throw new ConflictException("Payment has no gateway payment id."),
                Money.ToPaise(created.Amount), created.Id.ToString("N"), created.Reason), ct);
            return await FinishAsync(created.Id, gw.Status, gw.GatewayRefundId, null, ct);
        }
        catch (PaymentGatewayException ex)
        {
            logger.LogWarning("Gateway refused refund {RefundId}: {Message}", created.Id, ex.Message);
            await FinishAsync(created.Id, GatewayRefundStatus.Failed, null, ex.Message, ct);
            throw;
        }
    }

    private async Task<RefundDto> FinishAsync(Guid refundId, GatewayRefundStatus status, string? gatewayRefundId, string? failure, CancellationToken ct)
    {
        await ConcurrencyRetry.RunAsync(uow, async token =>
        {
            var refund = await payments.GetRefundAsync(refundId, token) ?? throw new NotFoundException("Refund not found.");
            var payment = await payments.GetByIdAsync(refund.PaymentId, token) ?? throw new NotFoundException("Payment not found.");
            refund.GatewayRefundId ??= gatewayRefundId;
            refund.FailureReason = failure ?? refund.FailureReason;
            await state.ApplyRefundResultAsync(refund, payment, status, "Api", token);
            return 0;
        }, ct);

        return (await payments.GetRefundAsync(refundId, ct))!.ToDto();
    }
}

using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;
using QuickBasket.Payments.Models;

namespace QuickBasket.Application.Payments;

public enum ApplyResult { Applied, NoChange, Mismatch }

/// <summary>
/// The single place where payment and order state is changed from gateway facts.
/// Both the browser verify call and the webhook handler go through here, so they cannot disagree.
/// Callers run it inside a transaction and save; it is idempotent for repeated facts.
/// </summary>
public sealed class PaymentStateUpdater(IPaymentRepository payments, IOrderRepository orders)
{
    private static bool IsSettled(PaymentStatus s) =>
        s is PaymentStatus.Captured or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;

    public async Task<ApplyResult> ApplyGatewayPaymentAsync(Payment payment, GatewayPayment gp, string source, CancellationToken ct)
    {
        // Never trust amounts or order links from the caller: compare with what we calculated.
        var amountMatches = gp.AmountInPaise == Money.ToPaise(payment.Amount) &&
                            string.Equals(gp.Currency, payment.Currency, StringComparison.OrdinalIgnoreCase);
        var orderMatches = payment.GatewayOrderId is null || payment.GatewayOrderId == gp.GatewayOrderId;

        if (!amountMatches || !orderMatches)
        {
            await AddTransactionAsync(payment, PaymentTransactionType.AmountMismatch, payment.Status, gp.GatewayPaymentId, source,
                $"Gateway reported {gp.AmountInPaise} {gp.Currency} for order {gp.GatewayOrderId}; expected {Money.ToPaise(payment.Amount)} {payment.Currency} for {payment.GatewayOrderId}.", ct);
            return ApplyResult.Mismatch;
        }

        switch (gp.Status)
        {
            case GatewayPaymentStatus.Captured:
                if (IsSettled(payment.Status)) return ApplyResult.NoChange; // duplicate delivery

                payment.GatewayPaymentId = gp.GatewayPaymentId;
                payment.Method = MapMethod(payment.Method, gp.Method);
                payment.Status = PaymentStatus.Captured;
                payment.PaidAtUtc = DateTime.UtcNow;
                payment.FailureReason = null;
                payment.UpdatedAtUtc = DateTime.UtcNow;

                var order = await orders.GetByIdAsync(payment.OrderId, ct);
                string message = "Payment captured.";
                if (order is { Status: OrderStatus.PendingPayment })
                {
                    order.Status = OrderStatus.Confirmed;
                    order.UpdatedAtUtc = DateTime.UtcNow;
                    order.Events.Add(new OrderEvent { Status = OrderStatus.Confirmed, Note = "Payment received. Order confirmed." });
                }
                else
                {
                    // Money arrived for an order that is no longer waiting for it (cancelled, or already paid by another attempt).
                    message = $"Payment captured while order was {order?.Status.ToString() ?? "missing"}. Manual review / refund required.";
                }
                await AddTransactionAsync(payment, PaymentTransactionType.Captured, PaymentStatus.Captured, gp.GatewayPaymentId, source, message, ct);
                return ApplyResult.Applied;

            case GatewayPaymentStatus.Authorized:
                if (payment.Status is not (PaymentStatus.Created or PaymentStatus.Pending)) return ApplyResult.NoChange;
                payment.GatewayPaymentId = gp.GatewayPaymentId;
                payment.Status = PaymentStatus.Authorized;
                payment.UpdatedAtUtc = DateTime.UtcNow;
                await AddTransactionAsync(payment, PaymentTransactionType.Authorized, PaymentStatus.Authorized, gp.GatewayPaymentId, source, "Payment authorized; awaiting capture.", ct);
                return ApplyResult.Applied;

            case GatewayPaymentStatus.Failed:
                // A failure can arrive after (or out of order with) a success; never downgrade a settled payment.
                if (payment.Status is not (PaymentStatus.Created or PaymentStatus.Pending or PaymentStatus.Authorized)) return ApplyResult.NoChange;
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = gp.ErrorDescription ?? "Payment failed.";
                payment.UpdatedAtUtc = DateTime.UtcNow;
                await AddTransactionAsync(payment, PaymentTransactionType.Failed, PaymentStatus.Failed, gp.GatewayPaymentId, source, payment.FailureReason, ct);
                return ApplyResult.Applied;

            default:
                return ApplyResult.NoChange;
        }
    }

    public async Task<bool> ExpireAsync(Payment payment, string source, CancellationToken ct)
    {
        if (payment.Status is not (PaymentStatus.Created or PaymentStatus.Pending)) return false;
        payment.Status = PaymentStatus.Expired;
        payment.FailureReason = "Payment window expired.";
        payment.UpdatedAtUtc = DateTime.UtcNow;
        await AddTransactionAsync(payment, PaymentTransactionType.Expired, PaymentStatus.Expired, null, source, "Payment window expired. The customer can retry.", ct);
        return true;
    }

    public async Task<bool> ApplyRefundResultAsync(Refund refund, Payment payment, GatewayRefundStatus status, string source, CancellationToken ct)
    {
        if (refund.Status != RefundStatus.Pending) return false; // already final: duplicate event

        if (status == GatewayRefundStatus.Processed)
        {
            refund.Status = RefundStatus.Processed;
            refund.ProcessedAtUtc = DateTime.UtcNow;
            payment.RefundedAmount += refund.Amount;
            payment.Status = payment.RefundedAmount >= payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
            payment.UpdatedAtUtc = DateTime.UtcNow;
            await AddTransactionAsync(payment, PaymentTransactionType.RefundProcessed, payment.Status, refund.GatewayRefundId, source,
                $"Refund of {refund.Amount} {refund.Currency} processed.", ct, refund.Amount);
            return true;
        }

        if (status == GatewayRefundStatus.Failed)
        {
            refund.Status = RefundStatus.Failed;
            refund.FailureReason ??= "Gateway reported the refund as failed.";
            await AddTransactionAsync(payment, PaymentTransactionType.RefundFailed, payment.Status, refund.GatewayRefundId, source, refund.FailureReason, ct, refund.Amount);
            return true;
        }
        return false; // still pending at the gateway
    }

    public Task AddTransactionAsync(Payment payment, PaymentTransactionType type, PaymentStatus statusAfter, string? reference,
        string source, string? message, CancellationToken ct, decimal? amount = null) =>
        payments.AddTransactionAsync(new PaymentTransaction
        {
            PaymentId = payment.Id, Type = type, StatusAfter = statusAfter,
            Amount = amount ?? payment.Amount, Currency = payment.Currency,
            GatewayReference = reference, Source = source, Message = message
        }, ct);

    private static PaymentMethod MapMethod(PaymentMethod current, string? gatewayMethod) => gatewayMethod?.ToLowerInvariant() switch
    {
        "upi" => current == PaymentMethod.UpiQr ? PaymentMethod.UpiQr : PaymentMethod.Upi,
        "card" => PaymentMethod.Card,
        "netbanking" => PaymentMethod.NetBanking,
        _ => current
    };
}

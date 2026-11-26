namespace QuickBasket.Payments.Models;

public enum GatewayPaymentStatus { Created, Authorized, Captured, Failed, Refunded }
public enum GatewayRefundStatus { Pending, Processed, Failed }

public sealed record CreateGatewayOrderRequest(
    long AmountInPaise, string Currency, string Receipt,
    IReadOnlyDictionary<string, string>? Notes = null);

public sealed record GatewayOrder(string GatewayOrderId, long AmountInPaise, string Currency, string Receipt);

public sealed record GatewayPayment(
    string GatewayPaymentId, string? GatewayOrderId, long AmountInPaise, string Currency,
    GatewayPaymentStatus Status, string? Method, string? ErrorDescription);

public sealed record CreateUpiQrRequest(
    long AmountInPaise, string Description, string Reference, DateTime ExpiresAtUtc,
    IReadOnlyDictionary<string, string>? Notes = null);

public sealed record GatewayUpiQr(string QrId, string ImageUrl, DateTime ExpiresAtUtc);

public sealed record RefundGatewayRequest(
    string GatewayPaymentId, long AmountInPaise, string IdempotencyKey, string? Reason);

public sealed record GatewayRefund(
    string GatewayRefundId, string GatewayPaymentId, long AmountInPaise, GatewayRefundStatus Status);

/// <summary>Provider-neutral view of a webhook. Only fields we act on are extracted.</summary>
public sealed record GatewayWebhookEvent(
    string EventId, string EventType,
    string? GatewayPaymentId, string? GatewayOrderId, string? GatewayQrId, string? GatewayRefundId,
    long? AmountInPaise, string? Currency,
    GatewayPaymentStatus? PaymentStatus, GatewayRefundStatus? RefundStatus,
    string? Method, string? ErrorDescription);

using QuickBasket.Domain.Enums;

namespace QuickBasket.Application.DTOs;

/// <param name="ConfirmedByCustomer">Must be true. The customer has to explicitly confirm the amount before any payment is created.</param>
public sealed record InitiatePaymentRequest(Guid OrderId, PaymentMethod Method, bool ConfirmedByCustomer);

public sealed record UpiQrDto(string ImageUrl, DateTime ExpiresAtUtc, decimal Amount, string Currency);

/// <summary>What the browser needs to open the gateway checkout. Contains only the public key id, never secrets.</summary>
public sealed record InitiatePaymentResponse(
    Guid PaymentId, Guid OrderId, string Provider, string? PublicKeyId, string? GatewayOrderId,
    decimal Amount, long AmountInPaise, string Currency, string Method, string Status,
    DateTime? ExpiresAtUtc, UpiQrDto? UpiQr);

/// <summary>Values the gateway checkout hands back to the browser. Treated as untrusted until verified server-side.</summary>
public sealed record VerifyPaymentRequest(Guid PaymentId, string GatewayOrderId, string GatewayPaymentId, string Signature);

public sealed record PaymentTransactionDto(string Type, string StatusAfter, decimal Amount, string Source, string? Message, DateTime OccurredAtUtc);
public sealed record RefundDto(Guid Id, Guid PaymentId, Guid OrderId, decimal Amount, string Currency, string Status, string? GatewayRefundId, string? Reason, DateTime CreatedAtUtc, DateTime? ProcessedAtUtc);

public sealed record PaymentDto(
    Guid Id, Guid OrderId, string Provider, string Method, string Status, decimal Amount, string Currency,
    string? GatewayOrderId, string? GatewayPaymentId, DateTime? ExpiresAtUtc, DateTime? PaidAtUtc,
    decimal RefundedAmount, string? FailureReason, DateTime CreatedAtUtc,
    IReadOnlyList<PaymentTransactionDto> Transactions, IReadOnlyList<RefundDto> Refunds);

public sealed record RefundRequest(decimal? Amount, string? Reason);

public sealed record WebhookResult(bool Accepted, string Outcome);

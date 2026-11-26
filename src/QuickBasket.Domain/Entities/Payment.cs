using QuickBasket.Domain.Common;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Domain.Entities;

/// <summary>One payment attempt for an order. A retry after failure/expiry creates a new Payment.</summary>
public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid UserId { get; set; }

    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";

    public string Provider { get; set; } = string.Empty;
    public string? GatewayOrderId { get; set; }
    public string? GatewayPaymentId { get; set; }
    public string? GatewayQrId { get; set; }
    public string? QrImageUrl { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }

    /// <summary>Client supplied key; unique per user so repeated submits return the same payment.</summary>
    public string? IdempotencyKey { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public decimal RefundedAmount { get; set; }

    /// <summary>Optimistic concurrency: stops two writers settling the same payment twice.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

/// <summary>Append-only audit trail of everything that happened to a payment.</summary>
public class PaymentTransaction : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public PaymentTransactionType Type { get; set; }
    public PaymentStatus StatusAfter { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string? GatewayReference { get; set; }
    /// <summary>Verify | Webhook | Api | System</summary>
    public string Source { get; set; } = "Api";
    public string? Message { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}

public class PaymentWebhookEvent : BaseEntity
{
    public string Provider { get; set; } = string.Empty;
    /// <summary>Gateway event id; (Provider, EventId) is unique so duplicates are detected.</summary>
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    /// <summary>Sanitised payload (card, vpa, email and contact fields removed).</summary>
    public string PayloadJson { get; set; } = string.Empty;
    public WebhookProcessingStatus Status { get; set; } = WebhookProcessingStatus.Received;
    public string? ErrorMessage { get; set; }
    public Guid? PaymentId { get; set; }
    public DateTime ReceivedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAtUtc { get; set; }
}

public class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public string? GatewayRefundId { get; set; }
    public string? Reason { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
}

namespace QuickBasket.Domain.Enums;

public enum UserRole { Customer = 0, Admin = 1, DeliveryPartner = 2 }

public enum OrderStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Packed = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum PaymentMethod
{
    Upi = 0,            // UPI intent (Google Pay, PhonePe, Paytm ...)
    UpiQr = 1,          // Dynamic, order-specific UPI QR
    Card = 2,
    NetBanking = 3,
    CashOnDelivery = 4
}

public enum PaymentStatus
{
    Created = 0,
    Pending = 1,        // awaiting customer action / COD awaiting collection
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Expired = 5,
    Cancelled = 6,
    PartiallyRefunded = 7,
    Refunded = 8
}

public enum PaymentTransactionType
{
    Initiated = 0,
    CheckoutVerified = 1,
    Authorized = 2,
    Captured = 3,
    Failed = 4,
    Expired = 5,
    AmountMismatch = 6,
    RefundRequested = 7,
    RefundProcessed = 8,
    RefundFailed = 9,
    CodCollected = 10
}

public enum RefundStatus { Pending = 0, Processed = 1, Failed = 2 }

public enum WebhookProcessingStatus { Received = 0, Processed = 1, Ignored = 2, Failed = 3 }

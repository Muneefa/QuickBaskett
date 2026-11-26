namespace QuickBasket.Application.Common;

public sealed class OrderPricingOptions
{
    public const string SectionName = "Pricing";
    public decimal DeliveryFee { get; set; } = 30m;
    public decimal FreeDeliveryThreshold { get; set; } = 499m;
}

public sealed class PaymentFlowOptions
{
    public const string SectionName = "Payments:Flow";
    /// <summary>How long a dynamic UPI QR stays payable. Gateways require at least a couple of minutes.</summary>
    public int QrExpiryMinutes { get; set; } = 10;
    /// <summary>How long a gateway order attempt stays open before it is marked expired and a retry is needed.</summary>
    public int AttemptExpiryMinutes { get; set; } = 30;
}

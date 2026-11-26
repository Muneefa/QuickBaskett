using QuickBasket.Payments.Models;

namespace QuickBasket.Payments.Abstractions;

/// <summary>
/// The only surface the rest of the application uses to talk to a payment gateway.
/// Implement this interface to add another provider (Cashfree, Stripe, ...).
/// </summary>
public interface IPaymentProvider
{
    string Name { get; }

    /// <summary>Public key id that the browser checkout needs. Never a secret.</summary>
    string PublicKeyId { get; }

    Task<GatewayOrder> CreateOrderAsync(CreateGatewayOrderRequest request, CancellationToken ct = default);
    Task<GatewayPayment> FetchPaymentAsync(string gatewayPaymentId, CancellationToken ct = default);
    Task<GatewayUpiQr> CreateUpiQrAsync(CreateUpiQrRequest request, CancellationToken ct = default);
    Task<GatewayRefund> RefundAsync(RefundGatewayRequest request, CancellationToken ct = default);

    /// <summary>Checks the signature returned to the browser after checkout.</summary>
    bool VerifyCheckoutSignature(string gatewayOrderId, string gatewayPaymentId, string signature);

    /// <summary>Checks a webhook signature against the exact raw request body.</summary>
    bool VerifyWebhookSignature(string rawBody, string? signature);

    GatewayWebhookEvent ParseWebhook(string rawBody, string? eventIdHeader);
}

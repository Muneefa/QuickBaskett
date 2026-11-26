using System.Collections.Concurrent;
using System.Text.Json;
using QuickBasket.Payments.Abstractions;
using QuickBasket.Payments.Models;
using QuickBasket.Payments.Razorpay;
using QuickBasket.Payments.Security;

namespace QuickBasket.Payments.Mock;

public sealed class MockPaymentOptions
{
    public const string SectionName = "Payments:Mock";
    public string SigningSecret { get; set; } = "mock-signing-secret";
    public string WebhookSecret { get; set; } = "mock-webhook-secret";
}

/// <summary>
/// Deterministic in-memory provider for automated tests and local runs without any gateway credentials.
/// Webhook bodies use the Razorpay shape so the real parser is exercised.
/// </summary>
public sealed class MockPaymentProvider : IPaymentProvider
{
    private readonly MockPaymentOptions _options;
    private readonly ConcurrentDictionary<string, GatewayOrder> _orders = new();
    private readonly ConcurrentDictionary<string, GatewayPayment> _payments = new();
    private readonly ConcurrentDictionary<string, GatewayRefund> _refunds = new();

    public MockPaymentProvider(MockPaymentOptions? options = null) => _options = options ?? new MockPaymentOptions();

    public string Name => "Mock";
    public string PublicKeyId => "mock_key_id";

    public Task<GatewayOrder> CreateOrderAsync(CreateGatewayOrderRequest request, CancellationToken ct = default)
    {
        var order = new GatewayOrder($"order_mock_{Guid.NewGuid():N}", request.AmountInPaise, request.Currency, request.Receipt);
        _orders[order.GatewayOrderId] = order;
        return Task.FromResult(order);
    }
    public Task<GatewayUpiQr> CreateUpiQrAsync(CreateUpiQrRequest request, CancellationToken ct = default) =>
    Task.FromResult(new GatewayUpiQr($"qr_mock_{Guid.NewGuid():N}", MockQrImage, request.ExpiresAtUtc));
    public Task<GatewayPayment> FetchPaymentAsync(string gatewayPaymentId, CancellationToken ct = default) =>
        _payments.TryGetValue(gatewayPaymentId, out var p)
            ? Task.FromResult(p)
            : throw new PaymentGatewayException("Payment not found.", 404);

    //public Task<GatewayUpiQr> CreateUpiQrAsync(CreateUpiQrRequest request, CancellationToken ct = default) =>
    //    Task.FromResult(new GatewayUpiQr($"qr_mock_{Guid.NewGuid():N}", "https://mock.invalid/qr.png", request.ExpiresAtUtc));
    private const string MockQrImage =
    "data:image/svg+xml;utf8,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 200'%3E%3Crect width='200' height='200' fill='white'/%3E%3Cg fill='black'%3E%3Crect x='10' y='10' width='50' height='50'/%3E%3Crect x='140' y='10' width='50' height='50'/%3E%3Crect x='10' y='140' width='50' height='50'/%3E%3Crect x='80' y='80' width='40' height='40'/%3E%3C/g%3E%3Ctext x='100' y='185' font-size='14' text-anchor='middle' fill='black'%3EMOCK QR%3C/text%3E%3C/svg%3E";

    public Task<GatewayRefund> RefundAsync(RefundGatewayRequest request, CancellationToken ct = default)
    {
        // Idempotent per key, like the real gateway.
        var refund = _refunds.GetOrAdd(request.IdempotencyKey, _ =>
            new GatewayRefund($"rfnd_mock_{Guid.NewGuid():N}", request.GatewayPaymentId, request.AmountInPaise, GatewayRefundStatus.Pending));
        return Task.FromResult(refund);
    }

    public bool VerifyCheckoutSignature(string gatewayOrderId, string gatewayPaymentId, string signature) =>
        SignatureHelper.FixedTimeEqualsHex(Sign($"{gatewayOrderId}|{gatewayPaymentId}"), signature);

    public bool VerifyWebhookSignature(string rawBody, string? signature) =>
        SignatureHelper.FixedTimeEqualsHex(SignatureHelper.ComputeHmacSha256Hex(_options.WebhookSecret, rawBody), signature);

    public GatewayWebhookEvent ParseWebhook(string rawBody, string? eventIdHeader) =>
        RazorpayWebhookParser.Parse(rawBody, eventIdHeader);

    // ---- Test helpers (not part of IPaymentProvider) ----

    public string Sign(string payload) => SignatureHelper.ComputeHmacSha256Hex(_options.SigningSecret, payload);

    public string SignWebhook(string rawBody) => SignatureHelper.ComputeHmacSha256Hex(_options.WebhookSecret, rawBody);

    /// <summary>Simulates a customer paying a gateway order; returns the payment as the gateway would report it.</summary>
    public GatewayPayment SimulatePayment(string gatewayOrderId, GatewayPaymentStatus status, string method = "upi")
    {
        var order = _orders[gatewayOrderId];
        var payment = new GatewayPayment($"pay_mock_{Guid.NewGuid():N}", gatewayOrderId, order.AmountInPaise,
            order.Currency, status, method, status == GatewayPaymentStatus.Failed ? "Mock failure" : null);
        _payments[payment.GatewayPaymentId] = payment;
        return payment;
    }

    public string SignCheckout(string gatewayOrderId, string gatewayPaymentId) => Sign($"{gatewayOrderId}|{gatewayPaymentId}");

    public string BuildPaymentWebhook(string eventType, GatewayPayment p) => JsonSerializer.Serialize(new
    {
        @event = eventType,
        payload = new
        {
            payment = new
            {
                entity = new
                {
                    id = p.GatewayPaymentId,
                    order_id = p.GatewayOrderId,
                    amount = p.AmountInPaise,
                    currency = p.Currency,
                    status = p.Status.ToString().ToLowerInvariant(),
                    method = p.Method,
                    error_description = p.ErrorDescription
                }
            }
        }
    });

    /// <summary>Simulates a UPI QR payment (QR payments have no gateway order, only a QR id).</summary>
    public GatewayPayment SimulateQrPayment(long amountInPaise)
    {
        var payment = new GatewayPayment($"pay_mock_{Guid.NewGuid():N}", null, amountInPaise, "INR", GatewayPaymentStatus.Captured, "upi", null);
        _payments[payment.GatewayPaymentId] = payment;
        return payment;
    }

    public string BuildQrCreditedWebhook(string qrId, GatewayPayment p) => JsonSerializer.Serialize(new
    {
        @event = "qr_code.credited",
        payload = new
        {
            payment = new { entity = new { id = p.GatewayPaymentId, amount = p.AmountInPaise, currency = p.Currency, status = "captured", method = "upi" } },
            qr_code = new { entity = new { id = qrId } }
        }
    });

    public bool HasOrder(string gatewayOrderId) => _orders.ContainsKey(gatewayOrderId);

    public string BuildRefundWebhook(string eventType, string gatewayRefundId, string gatewayPaymentId, long amountInPaise, string status) =>
        JsonSerializer.Serialize(new
        {
            @event = eventType,
            payload = new
            {
                refund = new { entity = new { id = gatewayRefundId, payment_id = gatewayPaymentId, amount = amountInPaise, currency = "INR", status } }
            }
        });
}

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using QuickBasket.Payments.Abstractions;
using QuickBasket.Payments.Models;
using QuickBasket.Payments.Security;

namespace QuickBasket.Payments.Razorpay;

/// <summary>
/// Razorpay implementation over the REST API (no SDK dependency).
/// Basic-auth credentials are attached to the HttpClient in DI; secrets never leave the server.
/// </summary>
public sealed class RazorpayPaymentProvider : IPaymentProvider
{
    private readonly HttpClient _http;
    private readonly RazorpayOptions _options;

    public RazorpayPaymentProvider(HttpClient http, IOptions<RazorpayOptions> options)
    {
        _http = http;
        _options = options.Value;
        Validate(_options);
    }

    public string Name => "Razorpay";
    public string PublicKeyId => _options.KeyId;

    public static void Validate(RazorpayOptions o)
    {
        if (string.IsNullOrWhiteSpace(o.KeyId) || string.IsNullOrWhiteSpace(o.KeySecret))
            throw new PaymentConfigurationException(
                "Razorpay credentials are not configured. Set Payments:Razorpay:KeyId/KeySecret via environment variables or Key Vault.");

        if (!o.AllowLive && !o.KeyId.StartsWith("rzp_test_", StringComparison.Ordinal))
            throw new PaymentConfigurationException(
                "Live Razorpay keys are blocked. Use rzp_test_ keys until gateway onboarding is approved and Payments:Razorpay:AllowLive is enabled.");
    }

    public async Task<GatewayOrder> CreateOrderAsync(CreateGatewayOrderRequest request, CancellationToken ct = default)
    {
        using var doc = await SendAsync(HttpMethod.Post, "/v1/orders", new Dictionary<string, object?>
        {
            ["amount"] = request.AmountInPaise,
            ["currency"] = request.Currency,
            ["receipt"] = request.Receipt,
            ["notes"] = request.Notes
        }, ct);

        var r = doc.RootElement;
        return new GatewayOrder(
            r.GetProperty("id").GetString()!,
            r.GetProperty("amount").GetInt64(),
            r.GetProperty("currency").GetString()!,
            RazorpayWebhookParser.Str(r, "receipt") ?? request.Receipt);
    }

    public async Task<GatewayPayment> FetchPaymentAsync(string gatewayPaymentId, CancellationToken ct = default)
    {
        using var doc = await SendAsync(HttpMethod.Get, $"/v1/payments/{Uri.EscapeDataString(gatewayPaymentId)}", null, ct);
        var r = doc.RootElement;
        return new GatewayPayment(
            r.GetProperty("id").GetString()!,
            RazorpayWebhookParser.Str(r, "order_id"),
            r.GetProperty("amount").GetInt64(),
            r.GetProperty("currency").GetString()!,
            RazorpayWebhookParser.MapPaymentStatus(RazorpayWebhookParser.Str(r, "status")) ?? GatewayPaymentStatus.Created,
            RazorpayWebhookParser.Str(r, "method"),
            RazorpayWebhookParser.Str(r, "error_description"));
    }

    public async Task<GatewayUpiQr> CreateUpiQrAsync(CreateUpiQrRequest request, CancellationToken ct = default)
    {
        var closeBy = new DateTimeOffset(DateTime.SpecifyKind(request.ExpiresAtUtc, DateTimeKind.Utc)).ToUnixTimeSeconds();
        using var doc = await SendAsync(HttpMethod.Post, "/v1/payments/qr_codes", new Dictionary<string, object?>
        {
            ["type"] = "upi_qr",
            ["name"] = "QuickBasket",
            ["usage"] = "single_use",
            ["fixed_amount"] = true,
            ["payment_amount"] = request.AmountInPaise,
            ["description"] = request.Description,
            ["close_by"] = closeBy,
            ["notes"] = request.Notes
        }, ct);

        var r = doc.RootElement;
        return new GatewayUpiQr(
            r.GetProperty("id").GetString()!,
            r.GetProperty("image_url").GetString()!,
            request.ExpiresAtUtc);
    }

    public async Task<GatewayRefund> RefundAsync(RefundGatewayRequest request, CancellationToken ct = default)
    {
        using var doc = await SendAsync(
            HttpMethod.Post,
            $"/v1/payments/{Uri.EscapeDataString(request.GatewayPaymentId)}/refund",
            new Dictionary<string, object?>
            {
                ["amount"] = request.AmountInPaise,
                ["speed"] = "normal",
                ["notes"] = new Dictionary<string, string> { ["reason"] = request.Reason ?? "" }
            }, ct, refundIdempotencyKey: request.IdempotencyKey);

        var r = doc.RootElement;
        return new GatewayRefund(
            r.GetProperty("id").GetString()!,
            request.GatewayPaymentId,
            r.GetProperty("amount").GetInt64(),
            RazorpayWebhookParser.MapRefundStatus(RazorpayWebhookParser.Str(r, "status")) ?? GatewayRefundStatus.Pending);
    }

    // Razorpay: signature = HMAC_SHA256(order_id + "|" + payment_id, key_secret)
    public bool VerifyCheckoutSignature(string gatewayOrderId, string gatewayPaymentId, string signature) =>
        SignatureHelper.FixedTimeEqualsHex(
            SignatureHelper.ComputeHmacSha256Hex(_options.KeySecret, $"{gatewayOrderId}|{gatewayPaymentId}"),
            signature);

    // Razorpay: X-Razorpay-Signature = HMAC_SHA256(raw request body, webhook_secret)
    public bool VerifyWebhookSignature(string rawBody, string? signature) =>
        !string.IsNullOrEmpty(_options.WebhookSecret) &&
        SignatureHelper.FixedTimeEqualsHex(SignatureHelper.ComputeHmacSha256Hex(_options.WebhookSecret, rawBody), signature);

    public GatewayWebhookEvent ParseWebhook(string rawBody, string? eventIdHeader) =>
        RazorpayWebhookParser.Parse(rawBody, eventIdHeader);

    private async Task<JsonDocument> SendAsync(
        HttpMethod method, string path, object? body, CancellationToken ct, string? refundIdempotencyKey = null)
    {
        using var req = new HttpRequestMessage(method, path);
        if (body is not null) req.Content = JsonContent.Create(body);
        if (refundIdempotencyKey is not null) req.Headers.TryAddWithoutValidation("X-Refund-Idempotency", refundIdempotencyKey);

        using var res = await _http.SendAsync(req, ct);
        var text = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
            throw new PaymentGatewayException(ExtractError(text) ?? $"Gateway returned HTTP {(int)res.StatusCode}.", (int)res.StatusCode);

        return JsonDocument.Parse(text);
    }

    private static string? ExtractError(string body)
    {
        try
        {
            using var d = JsonDocument.Parse(body);
            return RazorpayWebhookParser.Str(RazorpayWebhookParser.Path(d.RootElement, "error"), "description");
        }
        catch (JsonException) { return null; }
    }
}

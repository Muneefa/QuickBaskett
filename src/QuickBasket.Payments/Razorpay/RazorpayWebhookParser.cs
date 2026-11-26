using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QuickBasket.Payments.Models;

namespace QuickBasket.Payments.Razorpay;

/// <summary>Parses Razorpay-shaped webhook JSON. Also used by the mock provider so tests exercise the same path.</summary>
public static class RazorpayWebhookParser
{
    public static GatewayWebhookEvent Parse(string rawBody, string? eventIdHeader)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            var root = doc.RootElement;
            var eventType = Str(root, "event") ?? throw new InvalidWebhookException("Missing event type.");

            var payment = Path(root, "payload", "payment", "entity");
            var refund = Path(root, "payload", "refund", "entity");
            var qr = Path(root, "payload", "qr_code", "entity");

            // Razorpay sends a unique id in the X-Razorpay-Event-Id header. Fall back to a body hash so
            // identical redeliveries are still detected as duplicates.
            var eventId = !string.IsNullOrWhiteSpace(eventIdHeader)
                ? eventIdHeader!
                : "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawBody))).ToLowerInvariant();

            return new GatewayWebhookEvent(
                EventId: eventId,
                EventType: eventType,
                GatewayPaymentId: Str(payment, "id") ?? Str(refund, "payment_id"),
                GatewayOrderId: Str(payment, "order_id"),
                GatewayQrId: Str(qr, "id"),
                GatewayRefundId: Str(refund, "id"),
                AmountInPaise: Long(payment, "amount") ?? Long(refund, "amount"),
                Currency: Str(payment, "currency") ?? Str(refund, "currency"),
                PaymentStatus: payment.HasValue ? MapPaymentStatus(Str(payment, "status")) : null,
                RefundStatus: refund.HasValue ? MapRefundStatus(Str(refund, "status")) : null,
                Method: Str(payment, "method"),
                ErrorDescription: Str(payment, "error_description"));
        }
        catch (JsonException ex)
        {
            throw new InvalidWebhookException("Webhook body is not valid JSON.", ex);
        }
    }

    public static GatewayPaymentStatus? MapPaymentStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "created" => GatewayPaymentStatus.Created,
        "authorized" => GatewayPaymentStatus.Authorized,
        "captured" => GatewayPaymentStatus.Captured,
        "failed" => GatewayPaymentStatus.Failed,
        "refunded" => GatewayPaymentStatus.Refunded,
        _ => null
    };

    public static GatewayRefundStatus? MapRefundStatus(string? s) => s?.ToLowerInvariant() switch
    {
        "pending" => GatewayRefundStatus.Pending,
        "processed" => GatewayRefundStatus.Processed,
        "failed" => GatewayRefundStatus.Failed,
        _ => null
    };

    public static JsonElement? Path(JsonElement? start, params string[] path)
    {
        if (start is null) return null;
        var cur = start.Value;
        foreach (var p in path)
        {
            if (cur.ValueKind != JsonValueKind.Object || !cur.TryGetProperty(p, out var next)) return null;
            cur = next;
        }
        return cur;
    }

    public static string? Str(JsonElement? e, string name) =>
        e is { ValueKind: JsonValueKind.Object } v && v.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString() : null;

    public static long? Long(JsonElement? e, string name) =>
        e is { ValueKind: JsonValueKind.Object } v && v.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number
            ? p.GetInt64() : null;
}

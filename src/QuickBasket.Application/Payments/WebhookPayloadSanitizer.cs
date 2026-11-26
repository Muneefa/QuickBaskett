using System.Text.Json.Nodes;

namespace QuickBasket.Application.Payments;

/// <summary>
/// Data minimisation: webhook bodies are stored for audit, but customer-identifying and instrument fields are removed first.
/// We never store card numbers or CVV (gateways do not send them), and we drop card/vpa/email/contact anyway.
/// </summary>
public static class WebhookPayloadSanitizer
{
    private static readonly HashSet<string> Sensitive = new(StringComparer.OrdinalIgnoreCase)
    {
        "card", "card_id", "vpa", "email", "contact", "upi", "bank", "wallet", "acquirer_data", "customer_id"
    };

    public static string Sanitize(string rawJson)
    {
        try
        {
            var node = JsonNode.Parse(rawJson);
            Strip(node);
            return node?.ToJsonString() ?? "{}";
        }
        catch (System.Text.Json.JsonException)
        {
            return "{\"error\":\"unparseable payload\"}";
        }
    }

    private static void Strip(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(kv => kv.Key).Where(Sensitive.Contains).ToList()) obj.Remove(key);
                foreach (var child in obj.Select(kv => kv.Value).ToList()) Strip(child);
                break;
            case JsonArray arr:
                foreach (var child in arr) Strip(child);
                break;
        }
    }
}

namespace QuickBasket.Payments.Razorpay;

public sealed class RazorpayOptions
{
    public const string SectionName = "Payments:Razorpay";

    public string KeyId { get; set; } = string.Empty;
    /// <summary>Server-side only. Supply via environment variable or Azure Key Vault - never commit, never send to React.</summary>
    public string KeySecret { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.razorpay.com";

    /// <summary>
    /// Safety switch. While false, only keys starting with "rzp_test_" are accepted.
    /// Keep false until business onboarding is approved.
    /// </summary>
    public bool AllowLive { get; set; }
}

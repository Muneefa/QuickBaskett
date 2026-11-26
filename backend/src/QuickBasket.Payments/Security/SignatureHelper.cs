using System.Security.Cryptography;
using System.Text;

namespace QuickBasket.Payments.Security;

public static class SignatureHelper
{
    public static string ComputeHmacSha256Hex(string secret, string payload)
    {
        var hash = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>Constant-time comparison so response timing does not leak the expected signature.</summary>
    public static bool FixedTimeEqualsHex(string expectedHex, string? providedHex)
    {
        if (string.IsNullOrWhiteSpace(providedHex)) return false;
        var a = Encoding.UTF8.GetBytes(expectedHex.ToLowerInvariant());
        var b = Encoding.UTF8.GetBytes(providedHex.Trim().ToLowerInvariant());
        return a.Length == b.Length && CryptographicOperations.FixedTimeEquals(a, b);
    }
}

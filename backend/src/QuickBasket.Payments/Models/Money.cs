namespace QuickBasket.Payments.Models;

/// <summary>Gateways work in the smallest currency unit (paise). Convert in exactly one place.</summary>
public static class Money
{
    public static long ToPaise(decimal amount) =>
        checked((long)Math.Round(amount * 100m, 0, MidpointRounding.AwayFromZero));

    public static decimal FromPaise(long paise) => paise / 100m;
}

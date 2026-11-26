using QuickBasket.Payments.Mock;
using QuickBasket.Payments.Models;
using QuickBasket.Payments.Razorpay;
using QuickBasket.Payments.Security;
using Xunit;

namespace QuickBasket.Tests;

public class MoneyTests
{
    [Theory]
    [InlineData(199.99, 19999)]
    [InlineData(0.01, 1)]
    [InlineData(100, 10000)]
    [InlineData(10.005, 1001)] // half-up
    public void ToPaise_rounds_to_whole_paise(double rupees, long expected) =>
        Assert.Equal(expected, Money.ToPaise((decimal)rupees));
}

public class MockPaymentProviderTests
{
    private readonly MockPaymentProvider _provider = new();

    [Fact]
    public async Task Checkout_signature_is_verified()
    {
        var order = await _provider.CreateOrderAsync(new CreateGatewayOrderRequest(50000, "INR", "rcpt-1"));
        var payment = _provider.SimulatePayment(order.GatewayOrderId, GatewayPaymentStatus.Captured, "card");

        var good = _provider.SignCheckout(order.GatewayOrderId, payment.GatewayPaymentId);
        Assert.True(_provider.VerifyCheckoutSignature(order.GatewayOrderId, payment.GatewayPaymentId, good));
        Assert.False(_provider.VerifyCheckoutSignature(order.GatewayOrderId, payment.GatewayPaymentId, "deadbeef"));
        Assert.False(_provider.VerifyCheckoutSignature(order.GatewayOrderId, "pay_other", good));
    }

    [Fact]
    public async Task Webhook_signature_is_checked_against_the_exact_raw_body()
    {
        var order = await _provider.CreateOrderAsync(new CreateGatewayOrderRequest(50000, "INR", "rcpt-2"));
        var payment = _provider.SimulatePayment(order.GatewayOrderId, GatewayPaymentStatus.Captured);
        var body = _provider.BuildPaymentWebhook("payment.captured", payment);
        var signature = _provider.SignWebhook(body);

        Assert.True(_provider.VerifyWebhookSignature(body, signature));
        Assert.False(_provider.VerifyWebhookSignature(body + " ", signature)); // any byte change invalidates it
        Assert.False(_provider.VerifyWebhookSignature(body, null));
    }

    [Fact]
    public async Task Parsed_webhook_exposes_the_fields_the_application_acts_on()
    {
        var order = await _provider.CreateOrderAsync(new CreateGatewayOrderRequest(12345, "INR", "rcpt-3"));
        var payment = _provider.SimulatePayment(order.GatewayOrderId, GatewayPaymentStatus.Captured, "upi");

        var ev = _provider.ParseWebhook(_provider.BuildPaymentWebhook("payment.captured", payment), "evt_1");

        Assert.Equal("evt_1", ev.EventId);
        Assert.Equal("payment.captured", ev.EventType);
        Assert.Equal(payment.GatewayPaymentId, ev.GatewayPaymentId);
        Assert.Equal(order.GatewayOrderId, ev.GatewayOrderId);
        Assert.Equal(12345, ev.AmountInPaise);
        Assert.Equal(GatewayPaymentStatus.Captured, ev.PaymentStatus);
    }

    [Fact]
    public void Webhook_without_event_id_header_gets_a_stable_body_hash_id()
    {
        const string body = "{\"event\":\"payment.failed\",\"payload\":{}}";
        Assert.Equal(_provider.ParseWebhook(body, null).EventId, _provider.ParseWebhook(body, null).EventId);
    }

    [Fact]
    public void Invalid_json_is_rejected()
    {
        Assert.Throws<InvalidWebhookException>(() => _provider.ParseWebhook("not json", "evt"));
    }

    [Fact]
    public async Task Refund_with_same_idempotency_key_returns_the_same_refund()
    {
        var req = new RefundGatewayRequest("pay_1", 1000, "key-1", "test");
        var a = await _provider.RefundAsync(req);
        var b = await _provider.RefundAsync(req);
        Assert.Equal(a.GatewayRefundId, b.GatewayRefundId);
    }
}

public class RazorpayProviderTests
{
    [Fact]
    public void Live_keys_are_blocked_until_explicitly_allowed()
    {
        var live = new RazorpayOptions { KeyId = "rzp_live_abc", KeySecret = "s", AllowLive = false };
        Assert.Throws<PaymentConfigurationException>(() => RazorpayPaymentProvider.Validate(live));

        live.AllowLive = true;
        RazorpayPaymentProvider.Validate(live); // no exception
    }

    [Fact]
    public void Test_keys_are_accepted_and_missing_keys_rejected()
    {
        RazorpayPaymentProvider.Validate(new RazorpayOptions { KeyId = "rzp_test_abc", KeySecret = "s" });
        Assert.Throws<PaymentConfigurationException>(() => RazorpayPaymentProvider.Validate(new RazorpayOptions()));
    }

    [Fact]
    public void Checkout_and_webhook_signatures_follow_the_razorpay_scheme()
    {
        var opts = new RazorpayOptions { KeyId = "rzp_test_abc", KeySecret = "secret", WebhookSecret = "whsec" };
        var provider = new RazorpayPaymentProvider(new HttpClient(), Microsoft.Extensions.Options.Options.Create(opts));

        var checkoutSig = SignatureHelper.ComputeHmacSha256Hex("secret", "order_1|pay_1");
        Assert.True(provider.VerifyCheckoutSignature("order_1", "pay_1", checkoutSig));
        Assert.False(provider.VerifyCheckoutSignature("order_1", "pay_2", checkoutSig));

        const string body = "{\"event\":\"payment.captured\"}";
        Assert.True(provider.VerifyWebhookSignature(body, SignatureHelper.ComputeHmacSha256Hex("whsec", body)));
        Assert.False(provider.VerifyWebhookSignature(body, SignatureHelper.ComputeHmacSha256Hex("wrong", body)));
    }

    [Fact]
    public void Webhook_verification_fails_closed_when_no_secret_is_configured()
    {
        var opts = new RazorpayOptions { KeyId = "rzp_test_abc", KeySecret = "secret", WebhookSecret = "" };
        var provider = new RazorpayPaymentProvider(new HttpClient(), Microsoft.Extensions.Options.Options.Create(opts));
        Assert.False(provider.VerifyWebhookSignature("{}", SignatureHelper.ComputeHmacSha256Hex("", "{}")));
    }
}

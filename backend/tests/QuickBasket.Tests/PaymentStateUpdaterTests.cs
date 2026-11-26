using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Payments;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;
using QuickBasket.Payments.Models;
using Xunit;

namespace QuickBasket.Tests;

public class PaymentStateUpdaterTests
{
    private readonly FakePaymentRepository _payments = new();
    private readonly FakeOrderRepository _orders = new();
    private readonly PaymentStateUpdater _sut;

    public PaymentStateUpdaterTests() => _sut = new PaymentStateUpdater(_payments, _orders);

    private (Payment Payment, Order Order) Arrange(decimal amount = 499.50m)
    {
        var order = new Order { Status = OrderStatus.PendingPayment, TotalAmount = amount };
        var payment = new Payment
        {
            OrderId = order.Id, Amount = amount, Currency = "INR", Provider = "Mock",
            GatewayOrderId = "order_1", Method = PaymentMethod.Upi, Status = PaymentStatus.Created
        };
        _orders.Orders[order.Id] = order;
        return (payment, order);
    }

    private static GatewayPayment Gw(GatewayPaymentStatus status, long paise = 49950, string orderId = "order_1") =>
        new("pay_1", orderId, paise, "INR", status, "upi", status == GatewayPaymentStatus.Failed ? "declined" : null);

    [Fact]
    public async Task Captured_payment_confirms_the_order_once()
    {
        var (payment, order) = Arrange();

        var first = await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured), "Webhook", default);
        var second = await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured), "Webhook", default); // duplicate delivery

        Assert.Equal(ApplyResult.Applied, first);
        Assert.Equal(ApplyResult.NoChange, second);
        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Single(_payments.Transactions, t => t.Type == PaymentTransactionType.Captured); // not double-recorded
    }

    [Fact]
    public async Task Amount_mismatch_is_flagged_and_never_settles_the_payment()
    {
        var (payment, order) = Arrange();

        var result = await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured, paise: 100), "Verify", default);

        Assert.Equal(ApplyResult.Mismatch, result);
        Assert.Equal(PaymentStatus.Created, payment.Status);
        Assert.Equal(OrderStatus.PendingPayment, order.Status);
        Assert.Contains(_payments.Transactions, t => t.Type == PaymentTransactionType.AmountMismatch);
    }

    [Fact]
    public async Task Payment_for_a_different_gateway_order_is_rejected()
    {
        var (payment, _) = Arrange();
        var result = await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured, orderId: "order_other"), "Verify", default);
        Assert.Equal(ApplyResult.Mismatch, result);
    }

    [Fact]
    public async Task Late_failure_event_cannot_downgrade_a_captured_payment()
    {
        var (payment, order) = Arrange();
        await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured), "Webhook", default);

        var result = await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Failed), "Webhook", default);

        Assert.Equal(ApplyResult.NoChange, result);
        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task Failed_payment_can_still_be_settled_by_a_later_successful_attempt()
    {
        var (payment, order) = Arrange();
        await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Failed), "Webhook", default);
        Assert.Equal(PaymentStatus.Failed, payment.Status);

        await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured), "Webhook", default);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task Capture_after_the_order_was_cancelled_is_recorded_for_manual_refund_but_does_not_reopen_the_order()
    {
        var (payment, order) = Arrange();
        order.Status = OrderStatus.Cancelled;

        await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured), "Webhook", default);

        Assert.Equal(PaymentStatus.Captured, payment.Status);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Contains(_payments.Transactions, t => t.Message!.Contains("refund", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Expiry_only_applies_to_unpaid_attempts()
    {
        var (open, _) = Arrange();
        Assert.True(await _sut.ExpireAsync(open, "System", default));
        Assert.Equal(PaymentStatus.Expired, open.Status);

        var (paid, _) = Arrange();
        await _sut.ApplyGatewayPaymentAsync(paid, Gw(GatewayPaymentStatus.Captured), "Webhook", default);
        Assert.False(await _sut.ExpireAsync(paid, "System", default));
        Assert.Equal(PaymentStatus.Captured, paid.Status);
    }

    [Fact]
    public async Task Refunds_move_the_payment_to_partially_then_fully_refunded_and_are_idempotent()
    {
        var (payment, _) = Arrange(500m);
        await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured, 50000), "Webhook", default);

        var first = new Refund { PaymentId = payment.Id, Amount = 200m, Currency = "INR", GatewayRefundId = "rfnd_1" };
        Assert.True(await _sut.ApplyRefundResultAsync(first, payment, GatewayRefundStatus.Processed, "Webhook", default));
        Assert.Equal(PaymentStatus.PartiallyRefunded, payment.Status);
        Assert.Equal(200m, payment.RefundedAmount);

        // the same refund event delivered again changes nothing
        Assert.False(await _sut.ApplyRefundResultAsync(first, payment, GatewayRefundStatus.Processed, "Webhook", default));
        Assert.Equal(200m, payment.RefundedAmount);

        var second = new Refund { PaymentId = payment.Id, Amount = 300m, Currency = "INR", GatewayRefundId = "rfnd_2" };
        await _sut.ApplyRefundResultAsync(second, payment, GatewayRefundStatus.Processed, "Webhook", default);
        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Equal(500m, payment.RefundedAmount);
    }

    [Fact]
    public async Task Failed_refund_does_not_change_the_refunded_amount()
    {
        var (payment, _) = Arrange(500m);
        await _sut.ApplyGatewayPaymentAsync(payment, Gw(GatewayPaymentStatus.Captured, 50000), "Webhook", default);

        var refund = new Refund { PaymentId = payment.Id, Amount = 100m, Currency = "INR" };
        await _sut.ApplyRefundResultAsync(refund, payment, GatewayRefundStatus.Failed, "Webhook", default);

        Assert.Equal(RefundStatus.Failed, refund.Status);
        Assert.Equal(0m, payment.RefundedAmount);
        Assert.Equal(PaymentStatus.Captured, payment.Status);
    }
}

public class WebhookPayloadSanitizerTests
{
    [Fact]
    public void Customer_and_instrument_fields_are_removed_at_any_depth()
    {
        const string raw = """
        {"event":"payment.captured","payload":{"payment":{"entity":{"id":"pay_1","amount":100,"email":"a@b.c","contact":"+910000000000","vpa":"x@upi","card":{"last4":"1111"}}}}}
        """;

        var clean = WebhookPayloadSanitizer.Sanitize(raw);

        Assert.Contains("pay_1", clean);
        Assert.DoesNotContain("a@b.c", clean);
        Assert.DoesNotContain("x@upi", clean);
        Assert.DoesNotContain("1111", clean);
        Assert.DoesNotContain("+910000000000", clean);
    }
}

// ---- Minimal fakes: only the members the state updater touches are implemented ----

internal sealed class FakeOrderRepository : IOrderRepository
{
    public Dictionary<Guid, Order> Orders { get; } = new();
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(Orders.GetValueOrDefault(id));
    public Task<Order?> GetForUserAsync(Guid id, Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Order>> ListByUserAsync(Guid userId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Order>> ListByPartnerAsync(Guid partnerId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<(IReadOnlyList<Order> Items, int Total)> ListAllAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
    public Task AddAsync(Order order, CancellationToken ct = default) => throw new NotImplementedException();
}

internal sealed class FakePaymentRepository : IPaymentRepository
{
    public List<PaymentTransaction> Transactions { get; } = new();
    public Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken ct = default) { Transactions.Add(transaction); return Task.CompletedTask; }

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Payment?> GetByIdempotencyKeyAsync(Guid userId, string key, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Payment?> GetByGatewayOrderIdAsync(string gatewayOrderId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Payment?> GetByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Payment?> GetByGatewayQrIdAsync(string gatewayQrId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Payment>> ListForOrderAsync(Guid orderId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<(IReadOnlyList<Payment> Items, int Total)> ListAsync(PaymentStatus? status, int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<IReadOnlyList<Payment>> ListExpiredOpenAsync(DateTime nowUtc, int take, CancellationToken ct = default) => throw new NotImplementedException();
    public Task AddAsync(Payment payment, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<PaymentWebhookEvent?> GetWebhookEventAsync(string provider, string eventId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task AddWebhookEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Refund?> GetRefundAsync(Guid id, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Refund?> GetRefundByIdempotencyKeyAsync(string key, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Refund?> GetRefundByGatewayIdAsync(string gatewayRefundId, CancellationToken ct = default) => throw new NotImplementedException();
    public Task AddRefundAsync(Refund refund, CancellationToken ct = default) => throw new NotImplementedException();
}

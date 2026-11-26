using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Application.Payments;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;
using QuickBasket.Payments.Abstractions;
using QuickBasket.Payments.Models;

namespace QuickBasket.Application.Services;

public sealed class PaymentService(
    IPaymentRepository payments,
    IOrderRepository orders,
    IPaymentProvider provider,
    PaymentStateUpdater state,
    IUnitOfWork uow,
    IOptions<PaymentFlowOptions> flow,
    ILogger<PaymentService> logger) : IPaymentService
{
    private static readonly PaymentStatus[] OpenStatuses = [PaymentStatus.Created, PaymentStatus.Pending, PaymentStatus.Authorized];

    // Steps 2-5 of the payment workflow: the backend decides the amount, creates the pending payment and the gateway order.
    public async Task<InitiatePaymentResponse> InitiateAsync(
        Guid userId, InitiatePaymentRequest request, string? idempotencyKey, CancellationToken ct = default)
    {
        if (!request.ConfirmedByCustomer)
            throw new ValidationException("The customer must explicitly confirm the payment before it can start.");
        if (idempotencyKey is { Length: > 100 })
            throw new ValidationException("Idempotency-Key is too long.");

        // Same key + same order -> same payment, never a second charge attempt.
        if (!string.IsNullOrWhiteSpace(idempotencyKey) &&
            await payments.GetByIdempotencyKeyAsync(userId, idempotencyKey, ct) is { } replay)
        {
            if (replay.OrderId != request.OrderId) throw new ConflictException("This idempotency key was already used for a different order.");
            return ToResponse(replay);
        }

        var order = await orders.GetForUserAsync(request.OrderId, userId, ct) ?? throw new NotFoundException("Order not found.");
        if (order.Status != OrderStatus.PendingPayment)
            throw new ConflictException($"This order is {order.Status} and cannot accept a new payment.");

        var attempts = await payments.ListForOrderAsync(order.Id, ct);
        if (attempts.Any(p => p.Status is PaymentStatus.Captured or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded))
            throw new ConflictException("This order has already been paid.");

        var now = DateTime.UtcNow;

        // A live attempt with the same method is reused (page refresh, double click).
        var live = attempts.FirstOrDefault(p => p.Method == request.Method && OpenStatuses.Contains(p.Status) && (p.ExpiresAtUtc == null || p.ExpiresAtUtc > now));
        if (live is not null) return ToResponse(live);

        // Switching method: retire other open attempts so the customer cannot end up paying twice.
        var toCancel = attempts.Where(p => OpenStatuses.Contains(p.Status)).Select(p => p.Id).ToList();

        // Money amount always comes from the stored order, never from the request.
        var payment = new Payment
        {
            OrderId = order.Id, UserId = userId, Method = request.Method, Amount = order.TotalAmount,
            Currency = order.Currency, IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey
        };

        if (request.Method == PaymentMethod.CashOnDelivery)
        {
            payment.Provider = "COD";
            payment.Status = PaymentStatus.Pending; // collected by the delivery partner
        }
        else
        {
            payment.Provider = provider.Name;
            var amountInPaise = Money.ToPaise(order.TotalAmount);
            var notes = new Dictionary<string, string> { ["order_number"] = order.OrderNumber, ["payment_id"] = payment.Id.ToString() };

            if (request.Method == PaymentMethod.UpiQr)
            {
                var expires = now.AddMinutes(flow.Value.QrExpiryMinutes);
                var qr = await provider.CreateUpiQrAsync(
                    new CreateUpiQrRequest(amountInPaise, $"QuickBasket order {order.OrderNumber}", order.OrderNumber, expires, notes), ct);
                payment.GatewayQrId = qr.QrId;
                payment.QrImageUrl = qr.ImageUrl;
                payment.ExpiresAtUtc = qr.ExpiresAtUtc;
                payment.Status = PaymentStatus.Pending;
            }
            else
            {
                var receipt = $"{order.OrderNumber}-{attempts.Count + 1}"; // gateways cap receipt length (40 for Razorpay)
                var gwOrder = await provider.CreateOrderAsync(new CreateGatewayOrderRequest(amountInPaise, order.Currency, receipt, notes), ct);
                payment.GatewayOrderId = gwOrder.GatewayOrderId;
                payment.ExpiresAtUtc = now.AddMinutes(flow.Value.AttemptExpiryMinutes);
                payment.Status = PaymentStatus.Created;
            }
        }

        await uow.ExecuteInTransactionAsync(async token =>
        {
            foreach (var id in toCancel)
            {
                var old = await payments.GetByIdAsync(id, token);
                if (old is null || !OpenStatuses.Contains(old.Status)) continue;
                old.Status = PaymentStatus.Cancelled;
                await state.AddTransactionAsync(old, PaymentTransactionType.Failed, PaymentStatus.Cancelled, null, "Api", "Replaced by a new payment attempt.", token);
            }

            await payments.AddAsync(payment, token);
            await state.AddTransactionAsync(payment, PaymentTransactionType.Initiated, payment.Status, payment.GatewayOrderId ?? payment.GatewayQrId,
                "Api", $"{payment.Method} payment initiated for {payment.Amount} {payment.Currency}.", token);

            if (payment.Method == PaymentMethod.CashOnDelivery)
            {
                var o = await orders.GetByIdAsync(order.Id, token);
                if (o is { Status: OrderStatus.PendingPayment })
                {
                    o.Status = OrderStatus.Confirmed;
                    o.Events.Add(new OrderEvent { Status = OrderStatus.Confirmed, Note = "Cash on delivery selected. Order confirmed." });
                }
            }
            return 0;
        }, ct);

        logger.LogInformation("Payment {PaymentId} ({Method}) initiated for order {OrderId}", payment.Id, payment.Method, order.Id);
        return ToResponse(payment);
    }

    // Step 7: the browser says "paid". We treat that as a hint, check the signature, then ask the gateway directly.
    public async Task<PaymentDto> VerifyCheckoutAsync(Guid userId, VerifyPaymentRequest request, CancellationToken ct = default)
    {
        var existing = await payments.GetByIdAsync(request.PaymentId, ct);
        if (existing is null || existing.UserId != userId) throw new NotFoundException("Payment not found.");
        if (existing.GatewayOrderId is null || existing.GatewayOrderId != request.GatewayOrderId)
            throw new ValidationException("Payment does not match the gateway order.");

        if (!provider.VerifyCheckoutSignature(request.GatewayOrderId, request.GatewayPaymentId, request.Signature))
        {
            logger.LogWarning("Invalid checkout signature for payment {PaymentId}", request.PaymentId);
            throw new ValidationException("Payment signature could not be verified.");
        }

        // Authoritative status comes from the gateway API, not from the browser.
        var gatewayPayment = await provider.FetchPaymentAsync(request.GatewayPaymentId, ct);

        var result = await ConcurrencyRetry.RunAsync(uow, async token =>
        {
            var payment = await payments.GetByIdAsync(request.PaymentId, token) ?? throw new NotFoundException("Payment not found.");
            var applied = await state.ApplyGatewayPaymentAsync(payment, gatewayPayment, "Verify", token);
            if (applied == ApplyResult.Applied)
                await state.AddTransactionAsync(payment, PaymentTransactionType.CheckoutVerified, payment.Status, request.GatewayPaymentId, "Verify",
                    "Checkout signature verified and payment confirmed with the gateway.", token);
            return applied;
        }, ct);

        if (result == ApplyResult.Mismatch)
            throw new ConflictException("The gateway payment does not match this order. It has been flagged for review.");

        return (await payments.GetByIdAsync(request.PaymentId, ct))!.ToDto();
    }

    public async Task<PaymentDto> GetMineAsync(Guid userId, Guid paymentId, CancellationToken ct = default)
    {
        var p = await payments.GetByIdAsync(paymentId, ct);
        if (p is null || p.UserId != userId) throw new NotFoundException("Payment not found.");
        return p.ToDto();
    }

    public async Task<IReadOnlyList<PaymentDto>> ListForOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        _ = await orders.GetForUserAsync(orderId, userId, ct) ?? throw new NotFoundException("Order not found.");
        return (await payments.ListForOrderAsync(orderId, ct)).Select(p => p.ToDto()).ToList();
    }

    public async Task<PagedResult<PaymentDto>> ListAllAsync(PaymentStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await payments.ListAsync(status, page, pageSize, ct);
        return new PagedResult<PaymentDto>(items.Select(p => p.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<PaymentDto> GetAsAdminAsync(Guid paymentId, CancellationToken ct = default) =>
        (await payments.GetByIdAsync(paymentId, ct) ?? throw new NotFoundException("Payment not found.")).ToDto();

    /// <summary>Marks attempts whose window has passed as expired so the customer can retry. A late webhook can still settle them.</summary>
    public async Task<int> ExpireOverdueAsync(DateTime nowUtc, CancellationToken ct = default)
    {
        var due = await payments.ListExpiredOpenAsync(nowUtc, 100, ct);
        var ids = due.Select(p => p.Id).ToList();
        if (ids.Count == 0) return 0;

        return await ConcurrencyRetry.RunAsync(uow, async token =>
        {
            var count = 0;
            foreach (var id in ids)
            {
                var p = await payments.GetByIdAsync(id, token);
                if (p is not null && await state.ExpireAsync(p, "System", token)) count++;
            }
            return count;
        }, ct);
    }

    private InitiatePaymentResponse ToResponse(Payment p) => new(
        p.Id, p.OrderId, p.Provider,
        p.Method == PaymentMethod.CashOnDelivery ? null : provider.PublicKeyId,
        p.GatewayOrderId, p.Amount, Money.ToPaise(p.Amount), p.Currency, p.Method.ToString(), p.Status.ToString(), p.ExpiresAtUtc,
        p.GatewayQrId is null || p.QrImageUrl is null || p.ExpiresAtUtc is null
            ? null
            : new UpiQrDto(p.QrImageUrl, p.ExpiresAtUtc.Value, p.Amount, p.Currency));
}

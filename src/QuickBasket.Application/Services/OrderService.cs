using Microsoft.Extensions.Options;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Application.Services;

public sealed class OrderService(
    IOrderRepository orders,
    ICartRepository carts,
    IAddressRepository addresses,
    ICatalogRepository catalog,
    IUserRepository users,
    IPaymentRepository payments,
    IUnitOfWork uow,
    IOptions<OrderPricingOptions> pricing) : IOrderService
{
    // Allowed manual status transitions. PendingPayment -> Confirmed happens only through verified payment (or COD selection).
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Transitions = new()
    {
        [OrderStatus.PendingPayment] = [OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Packed, OrderStatus.Cancelled],
        [OrderStatus.Packed] = [OrderStatus.OutForDelivery, OrderStatus.Cancelled],
        [OrderStatus.OutForDelivery] = [OrderStatus.Delivered],
        [OrderStatus.Delivered] = [],
        [OrderStatus.Cancelled] = []
    };

    public static decimal CalculateDeliveryFee(decimal subtotal, OrderPricingOptions o) =>
        subtotal <= 0 || subtotal >= o.FreeDeliveryThreshold ? 0m : o.DeliveryFee;

    // ---------- Customer ----------

    public async Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken ct = default)
    {
        var order = await uow.ExecuteInTransactionAsync(async token =>
        {
            var address = await addresses.GetAsync(request.AddressId, userId, token) ?? throw new NotFoundException("Delivery address not found.");
            var cart = await carts.GetByUserAsync(userId, token);
            if (cart is null || cart.Items.Count == 0) throw new ValidationException("Your cart is empty.");

            // The server, not the client, decides prices and totals.
            var products = (await catalog.GetProductsByIdsAsync(cart.Items.Select(i => i.ProductId), token)).ToDictionary(p => p.Id);
            var o = new Order
            {
                OrderNumber = NewOrderNumber(),
                UserId = userId,
                DeliveryRecipient = address.RecipientName,
                DeliveryPhone = address.Phone,
                DeliveryAddress = string.Join(", ", new[] { address.Line1, address.Line2, address.City, address.State, address.PostalCode }
                    .Where(s => !string.IsNullOrWhiteSpace(s))),
                Notes = request.Notes
            };

            foreach (var ci in cart.Items)
            {
                if (!products.TryGetValue(ci.ProductId, out var p) || !p.IsActive)
                    throw new ConflictException("An item in your cart is no longer available.");
                if (p.StockQuantity < ci.Quantity)
                    throw new ConflictException($"Only {p.StockQuantity} of '{p.Name}' left in stock.");

                p.StockQuantity -= ci.Quantity; // reserve stock; restored on cancellation
                o.Items.Add(new OrderItem
                {
                    ProductId = p.Id, ProductName = p.Name, Unit = p.Unit, UnitPrice = p.Price,
                    Quantity = ci.Quantity, LineTotal = p.Price * ci.Quantity
                });
            }

            o.Subtotal = o.Items.Sum(i => i.LineTotal);
            o.DeliveryFee = CalculateDeliveryFee(o.Subtotal, pricing.Value);
            o.TotalAmount = o.Subtotal + o.DeliveryFee;
            o.Events.Add(new OrderEvent { Status = OrderStatus.PendingPayment, Note = "Order placed. Awaiting payment." });

            await orders.AddAsync(o, token);
            cart.Items.Clear();
            return o;
        }, ct);

        return (await orders.GetByIdAsync(order.Id, ct))!.ToDto();
    }

    public async Task<IReadOnlyList<OrderDto>> ListMineAsync(Guid userId, CancellationToken ct = default) =>
        (await orders.ListByUserAsync(userId, ct)).Select(o => o.ToDto()).ToList();

    public async Task<OrderDto> GetMineAsync(Guid userId, Guid orderId, CancellationToken ct = default) =>
        (await orders.GetForUserAsync(orderId, userId, ct) ?? throw new NotFoundException("Order not found.")).ToDto();

    public async Task<OrderDto> CancelMineAsync(Guid userId, Guid orderId, CancellationToken ct = default)
    {
        await uow.ExecuteInTransactionAsync(async token =>
        {
            var order = await orders.GetForUserAsync(orderId, userId, token) ?? throw new NotFoundException("Order not found.");
            if (order.Status != OrderStatus.PendingPayment)
                throw new ConflictException("Only unpaid orders can be cancelled here. Contact support for paid orders.");
            await CancelAsync(order, "Cancelled by customer before payment.", token);
            return 0;
        }, ct);
        return await GetMineAsync(userId, orderId, ct);
    }

    // ---------- Admin ----------

    public async Task<PagedResult<OrderDto>> ListAllAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await orders.ListAllAsync(status, page, pageSize, ct);
        return new PagedResult<OrderDto>(items.Select(o => o.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<OrderDto> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default)
    {
        await uow.ExecuteInTransactionAsync(async token =>
        {
            var order = await orders.GetByIdAsync(orderId, token) ?? throw new NotFoundException("Order not found.");
            EnsureTransition(order, request.Status);

            switch (request.Status)
            {
                case OrderStatus.Cancelled:
                    await CancelAsync(order, request.Note ?? "Cancelled by admin.", token);
                    break;
                case OrderStatus.Delivered:
                    await FinalizeDeliveryAsync(order, codCollected: true, token);
                    break;
                default:
                    SetStatus(order, request.Status, request.Note);
                    break;
            }
            return 0;
        }, ct);
        return (await orders.GetByIdAsync(orderId, ct))!.ToDto();
    }

    public async Task<OrderDto> AssignPartnerAsync(Guid orderId, Guid partnerId, CancellationToken ct = default)
    {
        var order = await orders.GetByIdAsync(orderId, ct) ?? throw new NotFoundException("Order not found.");
        var partner = await users.GetByIdAsync(partnerId, ct);
        if (partner is null || partner.Role != UserRole.DeliveryPartner || !partner.IsActive)
            throw new NotFoundException("Delivery partner not found.");
        if (order.Status is not (OrderStatus.Confirmed or OrderStatus.Packed or OrderStatus.OutForDelivery))
            throw new ConflictException("A partner can only be assigned to a confirmed order that is not yet delivered.");

        order.DeliveryPartnerId = partner.Id;
        order.DeliveryPartner = partner;
        order.UpdatedAtUtc = DateTime.UtcNow;
        order.Events.Add(new OrderEvent { Status = order.Status, Note = $"Assigned to delivery partner {partner.FullName}." });
        await uow.SaveChangesAsync(ct);
        return order.ToDto();
    }

    // ---------- Delivery partner ----------

    public async Task<IReadOnlyList<OrderDto>> ListAssignedAsync(Guid partnerId, CancellationToken ct = default) =>
        (await orders.ListByPartnerAsync(partnerId, ct)).Select(o => o.ToDto()).ToList();

    public async Task<OrderDto> PickUpAsync(Guid partnerId, Guid orderId, CancellationToken ct = default)
    {
        var order = await GetAssignedAsync(partnerId, orderId, ct);
        EnsureTransition(order, OrderStatus.OutForDelivery);
        SetStatus(order, OrderStatus.OutForDelivery, "Picked up by delivery partner.");
        await uow.SaveChangesAsync(ct);
        return order.ToDto();
    }

    public async Task<OrderDto> DeliverAsync(Guid partnerId, Guid orderId, DeliverOrderRequest request, CancellationToken ct = default)
    {
        await uow.ExecuteInTransactionAsync(async token =>
        {
            var order = await GetAssignedAsync(partnerId, orderId, token);
            EnsureTransition(order, OrderStatus.Delivered);
            await FinalizeDeliveryAsync(order, request.CodCollected, token);
            return 0;
        }, ct);
        return (await orders.GetByIdAsync(orderId, ct))!.ToDto();
    }

    // ---------- Helpers ----------

    private async Task<Order> GetAssignedAsync(Guid partnerId, Guid orderId, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(orderId, ct) ?? throw new NotFoundException("Order not found.");
        if (order.DeliveryPartnerId != partnerId) throw new ForbiddenException("This order is not assigned to you.");
        return order;
    }

    private static void EnsureTransition(Order order, OrderStatus next)
    {
        if (!Transitions[order.Status].Contains(next))
            throw new ConflictException($"An order that is {order.Status} cannot move to {next}.");
    }

    private static void SetStatus(Order order, OrderStatus status, string? note)
    {
        order.Status = status;
        order.UpdatedAtUtc = DateTime.UtcNow;
        order.Events.Add(new OrderEvent { Status = status, Note = note });
    }

    private async Task CancelAsync(Order order, string note, CancellationToken ct)
    {
        var products = (await catalog.GetProductsByIdsAsync(order.Items.Select(i => i.ProductId), ct)).ToDictionary(p => p.Id);
        foreach (var item in order.Items)
            if (products.TryGetValue(item.ProductId, out var p)) p.StockQuantity += item.Quantity;

        // Close any attempt still waiting for money so it cannot be paid against a cancelled order.
        foreach (var payment in await payments.ListForOrderAsync(order.Id, ct))
        {
            if (payment.Status is PaymentStatus.Created or PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Cancelled;
                await payments.AddTransactionAsync(new PaymentTransaction
                {
                    PaymentId = payment.Id, Type = PaymentTransactionType.Failed, StatusAfter = PaymentStatus.Cancelled,
                    Amount = payment.Amount, Currency = payment.Currency, Source = "System", Message = "Order cancelled."
                }, ct);
            }
        }
        SetStatus(order, OrderStatus.Cancelled, note);
    }

    private async Task FinalizeDeliveryAsync(Order order, bool codCollected, CancellationToken ct)
    {
        var all = await payments.ListForOrderAsync(order.Id, ct);
        var cod = all.FirstOrDefault(p => p.Method == PaymentMethod.CashOnDelivery && p.Status == PaymentStatus.Pending);
        if (cod is not null)
        {
            if (!codCollected) throw new ValidationException("Cash on delivery must be collected before marking the order delivered.");
            cod.Status = PaymentStatus.Captured;
            cod.PaidAtUtc = DateTime.UtcNow;
            await payments.AddTransactionAsync(new PaymentTransaction
            {
                PaymentId = cod.Id, Type = PaymentTransactionType.CodCollected, StatusAfter = PaymentStatus.Captured,
                Amount = cod.Amount, Currency = cod.Currency, Source = "Api", Message = "Cash collected on delivery."
            }, ct);
        }

        order.DeliveredAtUtc = DateTime.UtcNow;
        SetStatus(order, OrderStatus.Delivered, "Delivered.");
    }

    private static string NewOrderNumber() =>
        $"QB{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}

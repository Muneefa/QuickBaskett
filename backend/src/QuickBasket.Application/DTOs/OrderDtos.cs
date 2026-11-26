using QuickBasket.Domain.Enums;

namespace QuickBasket.Application.DTOs;

public sealed record CheckoutRequest(Guid AddressId, string? Notes);

public sealed record OrderItemDto(Guid ProductId, string ProductName, string Unit, decimal UnitPrice, int Quantity, decimal LineTotal);
public sealed record OrderEventDto(string Status, string? Note, DateTime OccurredAtUtc);
public sealed record DeliveryPartnerDto(Guid Id, string FullName, string? PhoneNumber);

public sealed record OrderDto(
    Guid Id, string OrderNumber, string Status, decimal Subtotal, decimal DeliveryFee, decimal TotalAmount, string Currency,
    string DeliveryRecipient, string DeliveryPhone, string DeliveryAddress, string? Notes,
    DeliveryPartnerDto? DeliveryPartner, DateTime CreatedAtUtc, DateTime? DeliveredAtUtc,
    string? LatestPaymentStatus,
    IReadOnlyList<OrderItemDto> Items, IReadOnlyList<OrderEventDto> Events);

public sealed record UpdateOrderStatusRequest(OrderStatus Status, string? Note);
public sealed record AssignPartnerRequest(Guid PartnerId);
public sealed record DeliverOrderRequest(bool CodCollected);

namespace QuickBasket.Application.DTOs;

public sealed record CartItemDto(Guid ProductId, string Name, string Unit, string? ImageUrl, decimal UnitPrice, int Quantity, decimal LineTotal);
public sealed record CartDto(IReadOnlyList<CartItemDto> Items, decimal Subtotal, decimal DeliveryFee, decimal Total, string Currency);
public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
public sealed record UpdateCartItemRequest(int Quantity);

public sealed record AddressDto(
    Guid Id, string Label, string RecipientName, string Phone, string Line1, string? Line2,
    string City, string State, string PostalCode, bool IsDefault);

public sealed record SaveAddressRequest(
    string Label, string RecipientName, string Phone, string Line1, string? Line2,
    string City, string State, string PostalCode, bool IsDefault);

using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Entities;
using QuickBasket.Payments.Models;

namespace QuickBasket.Application.Services;

internal static class Mappers
{
    public static UserDto ToDto(this User u) => new(u.Id, u.Email, u.FullName, u.PhoneNumber, u.Role.ToString(), u.IsAvailable);

    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name, c.Slug, c.ImageUrl, c.SortOrder, c.IsActive);

    public static ProductDto ToDto(this Product p) =>
        new(p.Id, p.CategoryId, p.Name, p.Slug, p.Description, p.ImageUrl, p.Price, p.MrpPrice, p.Unit, p.StockQuantity, p.IsActive);

    public static AddressDto ToDto(this Address a) =>
        new(a.Id, a.Label, a.RecipientName, a.Phone, a.Line1, a.Line2, a.City, a.State, a.PostalCode, a.IsDefault);

    public static OrderDto ToDto(this Order o) => new(
        o.Id, o.OrderNumber, o.Status.ToString(), o.Subtotal, o.DeliveryFee, o.TotalAmount, o.Currency,
        o.DeliveryRecipient, o.DeliveryPhone, o.DeliveryAddress, o.Notes,
        o.DeliveryPartner is null ? null : new DeliveryPartnerDto(o.DeliveryPartner.Id, o.DeliveryPartner.FullName, o.DeliveryPartner.PhoneNumber),
        o.CreatedAtUtc, o.DeliveredAtUtc,
        o.Payments.OrderByDescending(p => p.CreatedAtUtc).Select(p => p.Status.ToString()).FirstOrDefault(),
        o.Items.Select(i => new OrderItemDto(i.ProductId, i.ProductName, i.Unit, i.UnitPrice, i.Quantity, i.LineTotal)).ToList(),
        o.Events.OrderBy(e => e.OccurredAtUtc).Select(e => new OrderEventDto(e.Status.ToString(), e.Note, e.OccurredAtUtc)).ToList());

    public static RefundDto ToDto(this Refund r) =>
        new(r.Id, r.PaymentId, r.OrderId, r.Amount, r.Currency, r.Status.ToString(), r.GatewayRefundId, r.Reason, r.CreatedAtUtc, r.ProcessedAtUtc);

    public static PaymentDto ToDto(this Payment p) => new(
        p.Id, p.OrderId, p.Provider, p.Method.ToString(), p.Status.ToString(), p.Amount, p.Currency,
        p.GatewayOrderId, p.GatewayPaymentId, p.ExpiresAtUtc, p.PaidAtUtc, p.RefundedAmount, p.FailureReason, p.CreatedAtUtc,
        p.Transactions.OrderBy(t => t.OccurredAtUtc)
            .Select(t => new PaymentTransactionDto(t.Type.ToString(), t.StatusAfter.ToString(), t.Amount, t.Source, t.Message, t.OccurredAtUtc)).ToList(),
        p.Refunds.OrderBy(r => r.CreatedAtUtc).Select(r => r.ToDto()).ToList());

    public static string ToSlug(string value)
    {
        var chars = value.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
        return slug.Length == 0 ? Guid.NewGuid().ToString("N")[..8] : slug;
    }

    public static long ToPaise(decimal amount) => Money.ToPaise(amount);
}

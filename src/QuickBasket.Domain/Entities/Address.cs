using QuickBasket.Domain.Common;

namespace QuickBasket.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Label { get; set; } = "Home";
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

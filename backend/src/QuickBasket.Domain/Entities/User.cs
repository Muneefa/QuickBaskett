using QuickBasket.Domain.Common;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;

    /// <summary>Only meaningful for delivery partners.</summary>
    public bool IsAvailable { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}

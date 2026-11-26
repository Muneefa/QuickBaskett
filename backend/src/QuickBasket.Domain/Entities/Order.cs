using QuickBasket.Domain.Common;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User? User { get; set; }

    // Address snapshot - later edits to the address book must not change past orders.
    public string DeliveryRecipient { get; set; } = string.Empty;
    public string DeliveryPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    /// <summary>Authoritative amount, always calculated on the server.</summary>
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "INR";

    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;
    public Guid? DeliveryPartnerId { get; set; }
    public User? DeliveryPartner { get; set; }
    public string? Notes { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderEvent> Events { get; set; } = new List<OrderEvent>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderEvent : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order? Order { get; set; }
    public OrderStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
}

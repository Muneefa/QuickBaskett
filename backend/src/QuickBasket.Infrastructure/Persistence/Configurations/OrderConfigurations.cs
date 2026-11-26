using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBasket.Domain.Entities;

namespace QuickBasket.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasIndex(x => x.OrderNumber).IsUnique();
        b.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        b.HasIndex(x => x.DeliveryPartnerId);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DeliveryPartner).WithMany().HasForeignKey(x => x.DeliveryPartnerId).OnDelete(DeleteBehavior.Restrict);
        b.Property(x => x.OrderNumber).HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.DeliveryRecipient).HasMaxLength(200);
        b.Property(x => x.DeliveryPhone).HasMaxLength(32);
        b.Property(x => x.DeliveryAddress).HasMaxLength(1000);
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.Subtotal).HasPrecision(18, 2);
        b.Property(x => x.DeliveryFee).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("OrderItems");
        b.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.ProductName).HasMaxLength(200);
        b.Property(x => x.Unit).HasMaxLength(50);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.LineTotal).HasPrecision(18, 2);
    }
}

public class OrderEventConfiguration : IEntityTypeConfiguration<OrderEvent>
{
    public void Configure(EntityTypeBuilder<OrderEvent> b)
    {
        b.ToTable("OrderEvents");
        b.HasOne(x => x.Order).WithMany(o => o.Events).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Note).HasMaxLength(500);
    }
}

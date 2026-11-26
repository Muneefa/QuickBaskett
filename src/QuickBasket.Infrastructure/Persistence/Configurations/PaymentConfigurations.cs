using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickBasket.Domain.Entities;

namespace QuickBasket.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.HasOne(x => x.Order).WithMany(o => o.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Restrict);

        b.Property(x => x.Method).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.RefundedAmount).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.Provider).HasMaxLength(32).IsRequired();
        b.Property(x => x.GatewayOrderId).HasMaxLength(100);
        b.Property(x => x.GatewayPaymentId).HasMaxLength(100);
        b.Property(x => x.GatewayQrId).HasMaxLength(100);
        b.Property(x => x.QrImageUrl).HasMaxLength(500);
        b.Property(x => x.IdempotencyKey).HasMaxLength(100);
        b.Property(x => x.FailureReason).HasMaxLength(500);

        // Optimistic concurrency token: a second writer settling the same payment gets a conflict instead of a double update.
        b.Property(x => x.RowVersion).IsRowVersion();

        // Same key from the same user can only ever create one payment.
        b.HasIndex(x => new { x.UserId, x.IdempotencyKey }).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
        b.HasIndex(x => x.OrderId);
        b.HasIndex(x => x.GatewayOrderId);
        b.HasIndex(x => x.GatewayPaymentId);
        b.HasIndex(x => x.GatewayQrId);
        b.HasIndex(x => new { x.Status, x.ExpiresAtUtc });
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> b)
    {
        b.ToTable("PaymentTransactions");
        b.HasOne(x => x.Payment).WithMany(p => p.Transactions).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.PaymentId, x.OccurredAtUtc });
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.StatusAfter).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.GatewayReference).HasMaxLength(100);
        b.Property(x => x.Source).HasMaxLength(20);
        b.Property(x => x.Message).HasMaxLength(1000);
    }
}

public class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> b)
    {
        b.ToTable("PaymentWebhookEvents");
        // Duplicate delivery protection: the same event id from the same provider can be stored only once.
        b.HasIndex(x => new { x.Provider, x.EventId }).IsUnique();
        b.Property(x => x.Provider).HasMaxLength(32).IsRequired();
        b.Property(x => x.EventId).HasMaxLength(200).IsRequired();
        b.Property(x => x.EventType).HasMaxLength(100);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.ErrorMessage).HasMaxLength(1000);
        b.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> b)
    {
        b.ToTable("Refunds");
        b.HasOne(x => x.Payment).WithMany(p => p.Refunds).HasForeignKey(x => x.PaymentId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdempotencyKey).IsUnique();
        b.HasIndex(x => x.GatewayRefundId);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.GatewayRefundId).HasMaxLength(100);
        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.IdempotencyKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.FailureReason).HasMaxLength(500);
    }
}

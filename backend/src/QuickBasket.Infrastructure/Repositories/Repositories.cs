using Microsoft.EntityFrameworkCore;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;
using QuickBasket.Infrastructure.Persistence;

namespace QuickBasket.Infrastructure.Repositories;

public sealed class UserRepository(QuickBasketDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) => db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<User>> ListByRoleAsync(UserRole role, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().Where(u => u.Role == role).OrderBy(u => u.FullName).ToListAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default) => await db.Users.AddAsync(user, ct);
}

public sealed class AddressRepository(QuickBasketDbContext db) : IAddressRepository
{
    public async Task<IReadOnlyList<Address>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Addresses.Where(a => a.UserId == userId).OrderByDescending(a => a.IsDefault).ThenBy(a => a.CreatedAtUtc).ToListAsync(ct);

    public Task<Address?> GetAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

    public async Task AddAsync(Address address, CancellationToken ct = default) => await db.Addresses.AddAsync(address, ct);
    public void Remove(Address address) => db.Addresses.Remove(address);
}

public sealed class CatalogRepository(QuickBasketDbContext db) : ICatalogRepository
{
    public async Task<IReadOnlyList<Category>> ListCategoriesAsync(bool includeInactive, CancellationToken ct = default) =>
        await db.Categories.AsNoTracking().Where(c => includeInactive || c.IsActive).OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);

    public Task<Category?> GetCategoryAsync(Guid id, CancellationToken ct = default) => db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
    public async Task AddCategoryAsync(Category category, CancellationToken ct = default) => await db.Categories.AddAsync(category, ct);

    public async Task<(IReadOnlyList<Product> Items, int Total)> ListProductsAsync(ProductQuery q, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking().Where(p => q.IncludeInactive || p.IsActive);
        if (q.CategoryId is { } categoryId) query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(q.Search)) query = query.Where(p => p.Name.Contains(q.Search));

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(p => p.Name).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default) => db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();
        return await db.Products.Where(p => list.Contains(p.Id)).ToListAsync(ct);
    }

    public async Task AddProductAsync(Product product, CancellationToken ct = default) => await db.Products.AddAsync(product, ct);
}

public sealed class CartRepository(QuickBasketDbContext db) : ICartRepository
{
    public Task<Cart?> GetByUserAsync(Guid userId, CancellationToken ct = default) =>
        db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task AddAsync(Cart cart, CancellationToken ct = default) => await db.Carts.AddAsync(cart, ct);
}

public sealed class OrderRepository(QuickBasketDbContext db) : IOrderRepository
{
    private IQueryable<Order> Full() => db.Orders
        .Include(o => o.Items).Include(o => o.Events).Include(o => o.Payments).Include(o => o.DeliveryPartner)
        .AsSplitQuery();

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default) => Full().FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        Full().FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

    public async Task<IReadOnlyList<Order>> ListByUserAsync(Guid userId, CancellationToken ct = default) =>
        await Full().AsNoTracking().Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAtUtc).ToListAsync(ct);

    public async Task<IReadOnlyList<Order>> ListByPartnerAsync(Guid partnerId, CancellationToken ct = default) =>
        await Full().AsNoTracking().Where(o => o.DeliveryPartnerId == partnerId && o.Status != OrderStatus.Cancelled)
            .OrderByDescending(o => o.CreatedAtUtc).ToListAsync(ct);

    public async Task<(IReadOnlyList<Order> Items, int Total)> ListAllAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Full().AsNoTracking();
        if (status is { } s) query = query.Where(o => o.Status == s);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(o => o.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default) => await db.Orders.AddAsync(order, ct);
}

public sealed class PaymentRepository(QuickBasketDbContext db) : IPaymentRepository
{
    private IQueryable<Payment> Full() => db.Payments.Include(p => p.Transactions).Include(p => p.Refunds).AsSplitQuery();

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) => Full().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Payment?> GetByIdempotencyKeyAsync(Guid userId, string key, CancellationToken ct = default) =>
        Full().FirstOrDefaultAsync(p => p.UserId == userId && p.IdempotencyKey == key, ct);

    public Task<Payment?> GetByGatewayOrderIdAsync(string gatewayOrderId, CancellationToken ct = default) =>
        Full().FirstOrDefaultAsync(p => p.GatewayOrderId == gatewayOrderId, ct);

    public Task<Payment?> GetByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken ct = default) =>
        Full().FirstOrDefaultAsync(p => p.GatewayPaymentId == gatewayPaymentId, ct);

    public Task<Payment?> GetByGatewayQrIdAsync(string gatewayQrId, CancellationToken ct = default) =>
        Full().FirstOrDefaultAsync(p => p.GatewayQrId == gatewayQrId, ct);

    public async Task<IReadOnlyList<Payment>> ListForOrderAsync(Guid orderId, CancellationToken ct = default) =>
        await Full().Where(p => p.OrderId == orderId).OrderByDescending(p => p.CreatedAtUtc).ToListAsync(ct);

    public async Task<(IReadOnlyList<Payment> Items, int Total)> ListAsync(PaymentStatus? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Payments.AsNoTracking().Include(p => p.Refunds).AsQueryable();
        if (status is { } s) query = query.Where(p => p.Status == s);
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<Payment>> ListExpiredOpenAsync(DateTime nowUtc, int take, CancellationToken ct = default) =>
        await db.Payments.AsNoTracking()
            .Where(p => (p.Status == PaymentStatus.Created || p.Status == PaymentStatus.Pending) && p.ExpiresAtUtc != null && p.ExpiresAtUtc < nowUtc)
            .OrderBy(p => p.ExpiresAtUtc).Take(take).ToListAsync(ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default) => await db.Payments.AddAsync(payment, ct);
    public async Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken ct = default) => await db.PaymentTransactions.AddAsync(transaction, ct);

    public Task<PaymentWebhookEvent?> GetWebhookEventAsync(string provider, string eventId, CancellationToken ct = default) =>
        db.PaymentWebhookEvents.FirstOrDefaultAsync(e => e.Provider == provider && e.EventId == eventId, ct);

    public async Task AddWebhookEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken ct = default) =>
        await db.PaymentWebhookEvents.AddAsync(webhookEvent, ct);

    public Task<Refund?> GetRefundAsync(Guid id, CancellationToken ct = default) => db.Refunds.FirstOrDefaultAsync(r => r.Id == id, ct);
    public Task<Refund?> GetRefundByIdempotencyKeyAsync(string key, CancellationToken ct = default) => db.Refunds.FirstOrDefaultAsync(r => r.IdempotencyKey == key, ct);
    public Task<Refund?> GetRefundByGatewayIdAsync(string gatewayRefundId, CancellationToken ct = default) => db.Refunds.FirstOrDefaultAsync(r => r.GatewayRefundId == gatewayRefundId, ct);
    public async Task AddRefundAsync(Refund refund, CancellationToken ct = default) => await db.Refunds.AddAsync(refund, ct);
}

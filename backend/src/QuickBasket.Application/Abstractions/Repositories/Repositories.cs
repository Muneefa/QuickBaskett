using QuickBasket.Domain.Entities;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Application.Abstractions.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Runs the delegate in one database transaction, saves changes and commits on success; rolls back on any exception.</summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct = default);

    /// <summary>Drops tracked entities, used before re-reading after a concurrency conflict.</summary>
    void ClearTracking();
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListByRoleAsync(UserRole role, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}

public interface IAddressRepository
{
    Task<IReadOnlyList<Address>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Address?> GetAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task AddAsync(Address address, CancellationToken ct = default);
    void Remove(Address address);
}

public sealed record ProductQuery(Guid? CategoryId, string? Search, int Page, int PageSize, bool IncludeInactive = false);

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> ListCategoriesAsync(bool includeInactive, CancellationToken ct = default);
    Task<Category?> GetCategoryAsync(Guid id, CancellationToken ct = default);
    Task AddCategoryAsync(Category category, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> ListProductsAsync(ProductQuery query, CancellationToken ct = default);
    Task<Product?> GetProductAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task AddProductAsync(Product product, CancellationToken ct = default);
}

public interface ICartRepository
{
    Task<Cart?> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Cart cart, CancellationToken ct = default);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> ListByPartnerAsync(Guid partnerId, CancellationToken ct = default);
    Task<(IReadOnlyList<Order> Items, int Total)> ListAllAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
}

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Payment?> GetByIdempotencyKeyAsync(Guid userId, string key, CancellationToken ct = default);
    Task<Payment?> GetByGatewayOrderIdAsync(string gatewayOrderId, CancellationToken ct = default);
    Task<Payment?> GetByGatewayPaymentIdAsync(string gatewayPaymentId, CancellationToken ct = default);
    Task<Payment?> GetByGatewayQrIdAsync(string gatewayQrId, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> ListForOrderAsync(Guid orderId, CancellationToken ct = default);
    Task<(IReadOnlyList<Payment> Items, int Total)> ListAsync(PaymentStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> ListExpiredOpenAsync(DateTime nowUtc, int take, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken ct = default);

    Task<PaymentWebhookEvent?> GetWebhookEventAsync(string provider, string eventId, CancellationToken ct = default);
    Task AddWebhookEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken ct = default);

    Task<Refund?> GetRefundAsync(Guid id, CancellationToken ct = default);
    Task<Refund?> GetRefundByIdempotencyKeyAsync(string key, CancellationToken ct = default);
    Task<Refund?> GetRefundByGatewayIdAsync(string gatewayRefundId, CancellationToken ct = default);
    Task AddRefundAsync(Refund refund, CancellationToken ct = default);
}

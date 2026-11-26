using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Enums;

namespace QuickBasket.Application.Abstractions.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterCustomerAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserDto> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task<UserDto> CreatePartnerAsync(CreatePartnerRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<UserDto>> ListPartnersAsync(CancellationToken ct = default);
}

public interface ICatalogService
{
    Task<IReadOnlyList<CategoryDto>> ListCategoriesAsync(bool includeInactive, CancellationToken ct = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<PagedResult<ProductDto>> ListProductsAsync(Guid? categoryId, string? search, int page, int pageSize, CancellationToken ct = default);
    Task<ProductDto> GetProductAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
}

public interface ICartService
{
    Task<CartDto> GetAsync(Guid userId, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(Guid userId, AddCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> SetQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default);
    Task<CartDto> ClearAsync(Guid userId, CancellationToken ct = default);
}

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> ListAsync(Guid userId, CancellationToken ct = default);
    Task<AddressDto> CreateAsync(Guid userId, SaveAddressRequest request, CancellationToken ct = default);
    Task<AddressDto> UpdateAsync(Guid userId, Guid id, SaveAddressRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);
}

public interface IOrderService
{
    // Customer
    Task<OrderDto> CheckoutAsync(Guid userId, CheckoutRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<OrderDto>> ListMineAsync(Guid userId, CancellationToken ct = default);
    Task<OrderDto> GetMineAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<OrderDto> CancelMineAsync(Guid userId, Guid orderId, CancellationToken ct = default);

    // Admin
    Task<PagedResult<OrderDto>> ListAllAsync(OrderStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<OrderDto> UpdateStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct = default);
    Task<OrderDto> AssignPartnerAsync(Guid orderId, Guid partnerId, CancellationToken ct = default);

    // Delivery partner
    Task<IReadOnlyList<OrderDto>> ListAssignedAsync(Guid partnerId, CancellationToken ct = default);
    Task<OrderDto> PickUpAsync(Guid partnerId, Guid orderId, CancellationToken ct = default);
    Task<OrderDto> DeliverAsync(Guid partnerId, Guid orderId, DeliverOrderRequest request, CancellationToken ct = default);
}

public interface IPaymentService
{
    Task<InitiatePaymentResponse> InitiateAsync(Guid userId, InitiatePaymentRequest request, string? idempotencyKey, CancellationToken ct = default);
    Task<PaymentDto> VerifyCheckoutAsync(Guid userId, VerifyPaymentRequest request, CancellationToken ct = default);
    Task<PaymentDto> GetMineAsync(Guid userId, Guid paymentId, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDto>> ListForOrderAsync(Guid userId, Guid orderId, CancellationToken ct = default);
    Task<PagedResult<PaymentDto>> ListAllAsync(PaymentStatus? status, int page, int pageSize, CancellationToken ct = default);
    Task<PaymentDto> GetAsAdminAsync(Guid paymentId, CancellationToken ct = default);
    Task<int> ExpireOverdueAsync(DateTime nowUtc, CancellationToken ct = default);
}

public interface IPaymentWebhookService
{
    Task<WebhookResult> HandleAsync(string rawBody, string? signature, string? eventId, CancellationToken ct = default);
}

public interface IRefundService
{
    Task<RefundDto> RequestAsync(Guid adminUserId, Guid paymentId, RefundRequest request, string? idempotencyKey, CancellationToken ct = default);
}

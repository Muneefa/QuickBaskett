using Microsoft.Extensions.Options;
using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Entities;

namespace QuickBasket.Application.Services;

public sealed class CartService(ICartRepository carts, ICatalogRepository catalog, IUnitOfWork uow, IOptions<OrderPricingOptions> pricing) : ICartService
{
    private const int MaxQuantityPerItem = 50;

    public async Task<CartDto> GetAsync(Guid userId, CancellationToken ct = default) =>
        Build(await carts.GetByUserAsync(userId, ct));

    public async Task<CartDto> AddItemAsync(Guid userId, AddCartItemRequest r, CancellationToken ct = default)
    {
        if (r.Quantity <= 0) throw new ValidationException("Quantity must be at least 1.");
        var product = await RequireProductAsync(r.ProductId, ct);
        var cart = await GetOrCreateAsync(userId, ct);

        var item = cart.Items.FirstOrDefault(i => i.ProductId == r.ProductId);
        var newQty = (item?.Quantity ?? 0) + r.Quantity;
        EnsureQuantity(product, newQty);

        if (item is null) cart.Items.Add(new CartItem { CartId = cart.Id, ProductId = product.Id, Product = product, Quantity = newQty });
        else item.Quantity = newQty;

        await uow.SaveChangesAsync(ct);
        return Build(cart);
    }

    public async Task<CartDto> SetQuantityAsync(Guid userId, Guid productId, int quantity, CancellationToken ct = default)
    {
        var cart = await GetOrCreateAsync(userId, ct);
        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId) ?? throw new NotFoundException("Item is not in the cart.");

        if (quantity <= 0) cart.Items.Remove(item);
        else
        {
            EnsureQuantity(await RequireProductAsync(productId, ct), quantity);
            item.Quantity = quantity;
        }
        await uow.SaveChangesAsync(ct);
        return Build(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid userId, Guid productId, CancellationToken ct = default) =>
        await SetQuantityAsync(userId, productId, 0, ct);

    public async Task<CartDto> ClearAsync(Guid userId, CancellationToken ct = default)
    {
        var cart = await GetOrCreateAsync(userId, ct);
        cart.Items.Clear();
        await uow.SaveChangesAsync(ct);
        return Build(cart);
    }

    private async Task<Cart> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var cart = await carts.GetByUserAsync(userId, ct);
        if (cart is not null) return cart;
        cart = new Cart { UserId = userId };
        await carts.AddAsync(cart, ct);
        return cart;
    }

    private async Task<Product> RequireProductAsync(Guid id, CancellationToken ct)
    {
        var p = await catalog.GetProductAsync(id, ct);
        if (p is null || !p.IsActive) throw new NotFoundException("Product not found.");
        return p;
    }

    private static void EnsureQuantity(Product p, int qty)
    {
        if (qty > MaxQuantityPerItem) throw new ValidationException($"You can add at most {MaxQuantityPerItem} of an item.");
        if (qty > p.StockQuantity) throw new ConflictException($"Only {p.StockQuantity} of '{p.Name}' left in stock.");
    }

    private CartDto Build(Cart? cart)
    {
        var items = (cart?.Items ?? new List<CartItem>())
            .Where(i => i.Product is not null)
            .Select(i => new CartItemDto(i.ProductId, i.Product!.Name, i.Product.Unit, i.Product.ImageUrl, i.Product.Price, i.Quantity, i.Product.Price * i.Quantity))
            .ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var fee = OrderService.CalculateDeliveryFee(subtotal, pricing.Value);
        return new CartDto(items, subtotal, fee, subtotal + fee, "INR");
    }
}

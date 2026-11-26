using QuickBasket.Application.Abstractions.Repositories;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;
using QuickBasket.Domain.Entities;

namespace QuickBasket.Application.Services;

public sealed class CatalogService(ICatalogRepository catalog, IUnitOfWork uow) : ICatalogService
{
    public async Task<IReadOnlyList<CategoryDto>> ListCategoriesAsync(bool includeInactive, CancellationToken ct = default) =>
        (await catalog.ListCategoriesAsync(includeInactive, ct)).Select(c => c.ToDto()).ToList();

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest r, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(r.Name)) throw new ValidationException("Category name is required.");
        var category = new Category { Name = r.Name.Trim(), Slug = Mappers.ToSlug(r.Name), ImageUrl = r.ImageUrl, SortOrder = r.SortOrder };
        await catalog.AddCategoryAsync(category, ct);
        await uow.SaveChangesAsync(ct);
        return category.ToDto();
    }

    public async Task<PagedResult<ProductDto>> ListProductsAsync(Guid? categoryId, string? search, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var (items, total) = await catalog.ListProductsAsync(new ProductQuery(categoryId, search?.Trim(), page, pageSize), ct);
        return new PagedResult<ProductDto>(items.Select(p => p.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<ProductDto> GetProductAsync(Guid id, CancellationToken ct = default) =>
        (await catalog.GetProductAsync(id, ct) ?? throw new NotFoundException("Product not found.")).ToDto();

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest r, CancellationToken ct = default)
    {
        Validate(r.Name, r.Price, r.MrpPrice, r.StockQuantity);
        if (await catalog.GetCategoryAsync(r.CategoryId, ct) is null) throw new NotFoundException("Category not found.");

        var product = new Product
        {
            CategoryId = r.CategoryId, Name = r.Name.Trim(), Slug = Mappers.ToSlug(r.Name), Description = r.Description,
            ImageUrl = r.ImageUrl, Price = r.Price, MrpPrice = r.MrpPrice, Unit = r.Unit, StockQuantity = r.StockQuantity
        };
        await catalog.AddProductAsync(product, ct);
        await uow.SaveChangesAsync(ct);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest r, CancellationToken ct = default)
    {
        Validate(r.Name, r.Price, r.MrpPrice, r.StockQuantity);
        var product = await catalog.GetProductAsync(id, ct) ?? throw new NotFoundException("Product not found.");
        product.Name = r.Name.Trim();
        product.Description = r.Description;
        product.ImageUrl = r.ImageUrl;
        product.Price = r.Price;
        product.MrpPrice = r.MrpPrice;
        product.Unit = r.Unit;
        product.StockQuantity = r.StockQuantity;
        product.IsActive = r.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        return product.ToDto();
    }

    private static void Validate(string name, decimal price, decimal? mrp, int stock)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Product name is required.");
        if (price <= 0) throw new ValidationException("Price must be greater than zero.");
        if (mrp is { } m && m < price) throw new ValidationException("MRP cannot be lower than the selling price.");
        if (stock < 0) throw new ValidationException("Stock cannot be negative.");
    }
}

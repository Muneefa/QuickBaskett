namespace QuickBasket.Application.DTOs;

public sealed record CategoryDto(Guid Id, string Name, string Slug, string? ImageUrl, int SortOrder, bool IsActive);
public sealed record CreateCategoryRequest(string Name, string? ImageUrl, int SortOrder);

public sealed record ProductDto(
    Guid Id, Guid CategoryId, string Name, string Slug, string? Description, string? ImageUrl,
    decimal Price, decimal? MrpPrice, string Unit, int StockQuantity, bool IsActive);

public sealed record CreateProductRequest(
    Guid CategoryId, string Name, string? Description, string? ImageUrl,
    decimal Price, decimal? MrpPrice, string Unit, int StockQuantity);

public sealed record UpdateProductRequest(
    string Name, string? Description, string? ImageUrl,
    decimal Price, decimal? MrpPrice, string Unit, int StockQuantity, bool IsActive);

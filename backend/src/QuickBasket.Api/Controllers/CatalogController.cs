using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickBasket.Application.Abstractions.Services;
using QuickBasket.Application.Common;
using QuickBasket.Application.DTOs;

namespace QuickBasket.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class CatalogController(ICatalogService catalog) : ControllerBase
{
    [HttpGet("categories"), AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Categories(CancellationToken ct) =>
        Ok(await catalog.ListCategoriesAsync(includeInactive: false, ct));

    [HttpGet("products"), AllowAnonymous]
    public async Task<ActionResult<PagedResult<ProductDto>>> Products(
        [FromQuery] Guid? categoryId, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 24, CancellationToken ct = default) =>
        Ok(await catalog.ListProductsAsync(categoryId, search, page, pageSize, ct));

    [HttpGet("products/{id:guid}"), AllowAnonymous]
    public async Task<ActionResult<ProductDto>> Product(Guid id, CancellationToken ct) => Ok(await catalog.GetProductAsync(id, ct));

    [HttpPost("admin/categories"), Authorize(Policy = "Admin")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryRequest request, CancellationToken ct) =>
        Ok(await catalog.CreateCategoryAsync(request, ct));

    [HttpPost("admin/products"), Authorize(Policy = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductRequest request, CancellationToken ct) =>
        Ok(await catalog.CreateProductAsync(request, ct));

    [HttpPut("admin/products/{id:guid}"), Authorize(Policy = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, UpdateProductRequest request, CancellationToken ct) =>
        Ok(await catalog.UpdateProductAsync(id, request, ct));
}

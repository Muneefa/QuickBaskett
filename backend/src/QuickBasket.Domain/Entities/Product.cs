using QuickBasket.Domain.Common;

namespace QuickBasket.Domain.Entities;

public class Product : BaseEntity
{
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? MrpPrice { get; set; }
    public string Unit { get; set; } = "1 pc";
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
}

namespace Pdv.Web.Models;

/// <summary>Espelho de ProductSummaryDto.</summary>
public class ProductSummaryDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public int VariationCount { get; set; }
}

/// <summary>Espelho de ProductVariationDto.</summary>
public class ProductVariationDto
{
    public string Id { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>Espelho de ProductDetailDto.</summary>
public class ProductDetailDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
    public List<ProductVariationDto> Variations { get; set; } = [];
}

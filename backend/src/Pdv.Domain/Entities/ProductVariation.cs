namespace Pdv.Domain.Entities;

public sealed class ProductVariation : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

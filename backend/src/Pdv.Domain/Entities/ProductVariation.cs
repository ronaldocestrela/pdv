namespace Pdv.Domain.Entities;

public sealed class ProductVariation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
}

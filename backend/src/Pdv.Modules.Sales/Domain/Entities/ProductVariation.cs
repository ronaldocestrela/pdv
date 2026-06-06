namespace Pdv.Modules.Sales.Domain.Entities;

public sealed class ProductVariation
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
}

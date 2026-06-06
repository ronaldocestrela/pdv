namespace Pdv.Modules.Stock.Domain.Entities;

public sealed class ProductVariation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
}

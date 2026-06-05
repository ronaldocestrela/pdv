namespace Pdv.Domain.Entities;

public sealed class SaleItem : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public int ProductVariationId { get; set; }
    public ProductVariation ProductVariation { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

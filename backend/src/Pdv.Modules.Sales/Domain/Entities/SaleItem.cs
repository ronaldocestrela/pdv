using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Sales.Domain.Entities;

public sealed class SaleItem : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid SaleId { get; set; }
    public Sale Sale { get; set; } = null!;

    public Guid ProductVariationId { get; set; }
    public ProductVariation ProductVariation { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Catalog.Domain.Entities;

public sealed class ProductVariation : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public int StockQuantity { get; set; }
    public decimal UnitPrice { get; set; }
}

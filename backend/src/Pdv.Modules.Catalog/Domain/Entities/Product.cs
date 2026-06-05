using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Catalog.Domain.Entities;

public sealed class Product : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
}

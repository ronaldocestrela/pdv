using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Catalog.Domain.Entities;

public sealed class Product : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariation> Variations { get; set; } = new List<ProductVariation>();
}

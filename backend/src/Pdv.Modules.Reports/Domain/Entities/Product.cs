using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Reports.Domain.Entities;

public sealed class Product : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

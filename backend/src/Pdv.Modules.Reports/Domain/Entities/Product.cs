using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Reports.Domain.Entities;

public sealed class Product : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

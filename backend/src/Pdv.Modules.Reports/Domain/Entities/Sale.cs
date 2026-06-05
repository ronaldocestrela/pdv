using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Reports.Domain.Entities;

public sealed class Sale : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public DateTime CreatedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
}

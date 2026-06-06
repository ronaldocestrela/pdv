using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Reports.Domain.Entities;

public sealed class Sale : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
}

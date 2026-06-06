using Pdv.Shared.Kernel.Abstractions;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Reports.Domain.Entities;

public sealed class CashFlow : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public CashFlowType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public Guid? SaleId { get; set; }
}

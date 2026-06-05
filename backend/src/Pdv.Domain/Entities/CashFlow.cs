using Pdv.Domain.Enums;

namespace Pdv.Domain.Entities;

public sealed class CashFlow : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public CashFlowType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }

    public int? SaleId { get; set; }
    public Sale? Sale { get; set; }
}

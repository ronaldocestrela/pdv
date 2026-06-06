using Pdv.Shared.Kernel.Abstractions;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Sales.Domain.Entities;

public sealed class Sale : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<CashFlow> CashFlows { get; set; } = new List<CashFlow>();
}

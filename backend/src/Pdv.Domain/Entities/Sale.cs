using Pdv.Domain.Enums;

namespace Pdv.Domain.Entities;

public sealed class Sale : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public DateTime CreatedAtUtc { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<CashFlow> CashFlows { get; set; } = new List<CashFlow>();
}

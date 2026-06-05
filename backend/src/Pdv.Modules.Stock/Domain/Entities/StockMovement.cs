using Pdv.Shared.Kernel.Abstractions;
using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Stock.Domain.Entities;

public sealed class StockMovement : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public int ProductVariationId { get; set; }
    public ProductVariation ProductVariation { get; set; } = null!;

    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Reason { get; set; }
}

using Pdv.Domain.Enums;

namespace Pdv.Domain.Entities;

public sealed class StockMovement
{
    public int Id { get; set; }
    public int ProductVariationId { get; set; }
    public ProductVariation ProductVariation { get; set; } = null!;

    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string? Reason { get; set; }
}

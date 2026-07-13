namespace Pdv.Web.Models;

/// <summary>Espelho de StockMovementDto.</summary>
public class StockMovementDto
{
    public string Id { get; set; } = "";
    public string ProductVariationId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string VariationName { get; set; } = "";
    public string Type { get; set; } = "";
    public int Quantity { get; set; }
    public string CreatedAtUtc { get; set; } = "";
    public string? Reason { get; set; }
}

namespace Pdv.Web.Models;

/// <summary>Espelho de SalesReportDto.</summary>
public class SalesReportDto
{
    public int SaleCount { get; set; }
    public decimal TotalAmount { get; set; }
}

/// <summary>Espelho de TopProductReportDto.</summary>
public class TopProductReportDto
{
    public string ProductVariationId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string VariationName { get; set; } = "";
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>Espelho de CashFlowReportRowDto. Type: In=0, Out=1.</summary>
public class CashFlowReportRowDto
{
    public string Id { get; set; } = "";
    public int Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "";
    public string CreatedAtUtc { get; set; } = "";
    public string? SaleId { get; set; }
}

/// <summary>Espelho de StockReportRowDto.</summary>
public class StockReportRowDto
{
    public string ProductVariationId { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string VariationName { get; set; } = "";
    public int StockQuantity { get; set; }
}

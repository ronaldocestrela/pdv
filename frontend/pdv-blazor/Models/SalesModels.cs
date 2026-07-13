namespace Pdv.Web.Models;

/// <summary>Espelho de PaymentMethodDto — valores em camelCase conforme API JSON.</summary>
public enum PaymentMethod { Cash, Card, Pix }

/// <summary>Espelho de SaleLinePayload.</summary>
public class SaleLinePayload
{
    public string ProductVariationId { get; set; } = "";
    public int Quantity { get; set; }
}

/// <summary>Espelho de CreateSalePayload.</summary>
public class CreateSalePayload
{
    public List<SaleLinePayload> Items { get; set; } = [];
    public string PaymentMethod { get; set; } = "cash"; // "cash" | "card" | "pix"
}

/// <summary>Espelho de CreateSaleResultDto.</summary>
public class CreateSaleResultDto
{
    public string SaleId { get; set; } = "";
    public decimal TotalAmount { get; set; }
}

/// <summary>Espelho de SaleListItemDto.</summary>
public class SaleListItemDto
{
    public string Id { get; set; } = "";
    public string CreatedAtUtc { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public int ItemCount { get; set; }
}

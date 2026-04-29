namespace Pdv.API.Contracts;

public sealed record CreateVariationRequest(int ProductId, string Name, string? Barcode, int StockQuantity, decimal UnitPrice);

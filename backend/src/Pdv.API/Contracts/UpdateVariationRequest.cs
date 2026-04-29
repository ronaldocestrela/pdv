namespace Pdv.API.Contracts;

public sealed record UpdateVariationRequest(string Name, string? Barcode, int StockQuantity);

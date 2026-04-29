namespace Pdv.API.Contracts;

public sealed record AddStockRequest(int ProductVariationId, int Quantity, string? Reason);

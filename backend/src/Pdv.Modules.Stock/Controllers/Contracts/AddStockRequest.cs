namespace Pdv.Modules.Stock.Controllers.Contracts;

public sealed record AddStockRequest(int ProductVariationId, int Quantity, string? Reason);

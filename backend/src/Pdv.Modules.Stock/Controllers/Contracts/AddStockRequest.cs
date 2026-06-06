namespace Pdv.Modules.Stock.Controllers.Contracts;

public sealed record AddStockRequest(Guid ProductVariationId, int Quantity, string? Reason);

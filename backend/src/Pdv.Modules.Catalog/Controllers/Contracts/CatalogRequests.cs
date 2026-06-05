namespace Pdv.Modules.Catalog.Controllers.Contracts;

public sealed record CreateProductRequest(string Name, bool IsActive);

public sealed record UpdateProductRequest(string Name, bool IsActive);

public sealed record CreateVariationRequest(int ProductId, string Name, string? Barcode, int StockQuantity, decimal UnitPrice);

public sealed record UpdateVariationRequest(string Name, string? Barcode, int StockQuantity, decimal UnitPrice);

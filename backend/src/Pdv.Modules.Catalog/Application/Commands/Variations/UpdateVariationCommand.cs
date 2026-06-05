using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Variations;

public sealed record UpdateVariationCommand(int Id, string Name, string? Barcode, int StockQuantity, decimal UnitPrice) : IRequest<Unit>;

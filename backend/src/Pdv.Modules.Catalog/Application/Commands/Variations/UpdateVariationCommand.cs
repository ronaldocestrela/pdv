using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Variations;

public sealed record UpdateVariationCommand(Guid Id, string Name, string? Barcode, int StockQuantity, decimal UnitPrice) : IRequest<Unit>;

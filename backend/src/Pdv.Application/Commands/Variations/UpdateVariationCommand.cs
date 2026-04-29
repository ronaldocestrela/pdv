using MediatR;

namespace Pdv.Application.Commands.Variations;

public sealed record UpdateVariationCommand(int Id, string Name, string? Barcode, int StockQuantity) : IRequest<Unit>;

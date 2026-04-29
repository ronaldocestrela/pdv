using MediatR;

namespace Pdv.Application.Commands.Variations;

public sealed record CreateVariationCommand(int ProductId, string Name, string? Barcode, int StockQuantity, decimal UnitPrice) : IRequest<int>;

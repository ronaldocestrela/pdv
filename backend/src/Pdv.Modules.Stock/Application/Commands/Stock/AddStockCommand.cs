using MediatR;

namespace Pdv.Modules.Stock.Application.Commands.Stock;

public sealed record AddStockCommand(Guid ProductVariationId, int Quantity, string? Reason) : IRequest<Unit>;

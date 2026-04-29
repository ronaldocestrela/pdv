using MediatR;

namespace Pdv.Application.Commands.Stock;

public sealed record AddStockCommand(int ProductVariationId, int Quantity, string? Reason) : IRequest<Unit>;

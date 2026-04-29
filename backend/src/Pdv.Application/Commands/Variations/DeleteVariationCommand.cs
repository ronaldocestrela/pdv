using MediatR;

namespace Pdv.Application.Commands.Variations;

public sealed record DeleteVariationCommand(int Id) : IRequest<Unit>;

using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Variations;

public sealed record DeleteVariationCommand(Guid Id) : IRequest<Unit>;

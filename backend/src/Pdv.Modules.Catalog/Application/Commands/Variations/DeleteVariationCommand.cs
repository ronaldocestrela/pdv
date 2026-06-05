using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Variations;

public sealed record DeleteVariationCommand(int Id) : IRequest<Unit>;

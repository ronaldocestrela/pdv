using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Products;

public sealed record UpdateProductCommand(Guid Id, string Name, bool IsActive) : IRequest<Unit>;

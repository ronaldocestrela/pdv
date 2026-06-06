using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Products;

public sealed record CreateProductCommand(string Name, bool IsActive) : IRequest<Guid>;

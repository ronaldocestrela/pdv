using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Products;

public sealed record UpdateProductCommand(int Id, string Name, bool IsActive) : IRequest<Unit>;

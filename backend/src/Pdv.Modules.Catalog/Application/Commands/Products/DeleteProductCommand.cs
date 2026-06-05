using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Products;

public sealed record DeleteProductCommand(int Id) : IRequest<Unit>;

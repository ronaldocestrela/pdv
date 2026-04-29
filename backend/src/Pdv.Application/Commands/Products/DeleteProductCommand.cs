using MediatR;

namespace Pdv.Application.Commands.Products;

public sealed record DeleteProductCommand(int Id) : IRequest<Unit>;

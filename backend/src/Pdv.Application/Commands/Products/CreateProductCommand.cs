using MediatR;

namespace Pdv.Application.Commands.Products;

public sealed record CreateProductCommand(string Name, bool IsActive) : IRequest<int>;

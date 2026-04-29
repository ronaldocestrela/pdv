using MediatR;

namespace Pdv.Application.Commands.Products;

public sealed record UpdateProductCommand(int Id, string Name, bool IsActive) : IRequest<Unit>;

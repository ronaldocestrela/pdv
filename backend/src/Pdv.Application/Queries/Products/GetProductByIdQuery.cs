using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Products;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDetailDto?>;

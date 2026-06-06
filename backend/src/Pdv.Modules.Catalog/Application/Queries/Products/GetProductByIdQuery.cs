using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;

namespace Pdv.Modules.Catalog.Application.Queries.Products;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDetailDto?>;

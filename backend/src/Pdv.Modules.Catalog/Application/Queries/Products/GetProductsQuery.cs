using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;

namespace Pdv.Modules.Catalog.Application.Queries.Products;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductSummaryDto>>;

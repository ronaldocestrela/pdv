using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Products;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<ProductSummaryDto>>;

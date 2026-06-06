using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;

namespace Pdv.Modules.Catalog.Application.Queries.Suppliers;

/// <summary>
/// Query to retrieve summaries of all suppliers.
/// </summary>
public sealed record GetSuppliersQuery : IRequest<IReadOnlyList<SupplierSummaryDto>>;

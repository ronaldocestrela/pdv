using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;

namespace Pdv.Modules.Catalog.Application.Queries.Suppliers;

/// <summary>
/// Query to retrieve a supplier summary by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the supplier.</param>
public sealed record GetSupplierByIdQuery(Guid Id) : IRequest<SupplierSummaryDto?>;

using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Queries.Suppliers;

namespace Pdv.Modules.Catalog.Application.Handlers.Suppliers;

/// <summary>
/// Handler for executing the <see cref="GetSuppliersQuery"/>.
/// </summary>
public sealed class GetSuppliersQueryHandler(ICatalogRepository catalogRepository) 
    : IRequestHandler<GetSuppliersQuery, IReadOnlyList<SupplierSummaryDto>>
{
    private readonly ICatalogRepository _catalogRepository = catalogRepository;

    /// <summary>
    /// Handles the query, listing all suppliers in the tenant.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of supplier summaries.</returns>
    public Task<IReadOnlyList<SupplierSummaryDto>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken) =>
        _catalogRepository.ListSuppliersAsync(cancellationToken);
}

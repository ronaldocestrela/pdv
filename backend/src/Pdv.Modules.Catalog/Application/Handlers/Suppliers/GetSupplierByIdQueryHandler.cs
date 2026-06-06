using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Queries.Suppliers;

namespace Pdv.Modules.Catalog.Application.Handlers.Suppliers;

/// <summary>
/// Handler for executing the <see cref="GetSupplierByIdQuery"/>.
/// </summary>
public sealed class GetSupplierByIdQueryHandler(ICatalogRepository catalogRepository) 
    : IRequestHandler<GetSupplierByIdQuery, SupplierSummaryDto?>
{
    private readonly ICatalogRepository _catalogRepository = catalogRepository;

    /// <summary>
    /// Handles the query, finding a supplier by its ID.
    /// </summary>
    /// <param name="request">The query request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The supplier summary DTO if found; otherwise, null.</returns>
    public Task<SupplierSummaryDto?> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken) =>
        _catalogRepository.GetSupplierDetailByIdAsync(request.Id, cancellationToken);
}

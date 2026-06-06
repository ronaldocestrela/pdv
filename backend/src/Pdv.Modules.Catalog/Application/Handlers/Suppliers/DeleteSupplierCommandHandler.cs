using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;

namespace Pdv.Modules.Catalog.Application.Handlers.Suppliers;

/// <summary>
/// Handler for deleting an existing supplier.
/// </summary>
public sealed class DeleteSupplierCommandHandler(ICatalogRepository catalogRepository) 
    : IRequestHandler<DeleteSupplierCommand, Unit>
{
    private readonly ICatalogRepository _catalogRepository = catalogRepository;

    /// <summary>
    /// Handles the deletion of a supplier, validating that the supplier exists.
    /// </summary>
    /// <param name="request">The delete command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unit response.</returns>
    public async Task<Unit> Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _catalogRepository.GetTrackedSupplierByIdAsync(request.Id, cancellationToken);
        if (supplier is null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.Id), "Fornecedor não encontrado.")]);
        }

        _catalogRepository.RemoveSupplier(supplier);
        await _catalogRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

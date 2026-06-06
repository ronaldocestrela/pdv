using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;

namespace Pdv.Modules.Catalog.Application.Handlers.Suppliers;

/// <summary>
/// Handler for updating an existing supplier.
/// </summary>
public sealed class UpdateSupplierCommandHandler(ICatalogRepository catalogRepository) 
    : IRequestHandler<UpdateSupplierCommand, Unit>
{
    private readonly ICatalogRepository _catalogRepository = catalogRepository;

    /// <summary>
    /// Handles the update of a supplier, validating that the supplier exists and the document is not duplicated.
    /// </summary>
    /// <param name="request">The update command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unit response.</returns>
    public async Task<Unit> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _catalogRepository.GetTrackedSupplierByIdAsync(request.Id, cancellationToken);
        if (supplier is null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.Id), "Fornecedor não encontrado.")]);
        }

        var document = Normalize(request.Document);

        if (document is not null && await _catalogRepository.IsSupplierDocumentTakenAsync(document, request.Id, cancellationToken))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.Document), "Documento já cadastrado para outro fornecedor.")]);
        }

        supplier.Name = request.Name.Trim();
        supplier.Document = document;
        supplier.Email = Normalize(request.Email);
        supplier.Phone = Normalize(request.Phone);
        supplier.IsActive = request.IsActive;

        await _catalogRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private static string? Normalize(string? val) =>
        string.IsNullOrWhiteSpace(val) ? null : val.Trim();
}

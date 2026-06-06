using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;
using Pdv.Modules.Catalog.Domain.Entities;

namespace Pdv.Modules.Catalog.Application.Handlers.Suppliers;

/// <summary>
/// Handler for creating a new supplier.
/// </summary>
public sealed class CreateSupplierCommandHandler(ICatalogRepository catalogRepository) 
    : IRequestHandler<CreateSupplierCommand, Guid>
{
    private readonly ICatalogRepository _catalogRepository = catalogRepository;

    /// <summary>
    /// Handles the creation of a supplier, validating that the document number is unique.
    /// </summary>
    /// <param name="request">The create command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated unique identifier of the newly created supplier.</returns>
    public async Task<Guid> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var document = Normalize(request.Document);
        
        if (document is not null && await _catalogRepository.IsSupplierDocumentTakenAsync(document, null, cancellationToken))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(request.Document), "Documento já cadastrado para outro fornecedor.")]);
        }

        var supplier = new Supplier
        {
            Name = request.Name.Trim(),
            Document = document,
            Email = Normalize(request.Email),
            Phone = Normalize(request.Phone),
            IsActive = request.IsActive
        };

        _catalogRepository.AddSupplier(supplier);
        await _catalogRepository.SaveChangesAsync(cancellationToken);
        
        return supplier.Id;
    }

    private static string? Normalize(string? val) =>
        string.IsNullOrWhiteSpace(val) ? null : val.Trim();
}

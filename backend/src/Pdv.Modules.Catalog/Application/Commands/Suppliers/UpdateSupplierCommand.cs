using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Suppliers;

/// <summary>
/// Command to update an existing supplier (fornecedor).
/// </summary>
/// <param name="Id">The unique identifier of the supplier to update.</param>
/// <param name="Name">The updated name of the supplier.</param>
/// <param name="Document">Optional updated document (CPF/CNPJ) of the supplier.</param>
/// <param name="Email">Optional updated contact email of the supplier.</param>
/// <param name="Phone">Optional updated contact phone number of the supplier.</param>
/// <param name="IsActive">Indicates whether the supplier is active.</param>
public sealed record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? Document,
    string? Email,
    string? Phone,
    bool IsActive) : IRequest<Unit>;

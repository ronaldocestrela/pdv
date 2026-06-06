using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Suppliers;

/// <summary>
/// Command to register a new supplier (fornecedor).
/// </summary>
/// <param name="Name">The name of the supplier.</param>
/// <param name="Document">Optional document (CPF/CNPJ) of the supplier.</param>
/// <param name="Email">Optional contact email of the supplier.</param>
/// <param name="Phone">Optional contact phone number of the supplier.</param>
/// <param name="IsActive">Indicates whether the supplier is active.</param>
public sealed record CreateSupplierCommand(
    string Name,
    string? Document,
    string? Email,
    string? Phone,
    bool IsActive) : IRequest<Guid>;

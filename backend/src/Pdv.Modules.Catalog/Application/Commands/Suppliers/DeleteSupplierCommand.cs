using MediatR;

namespace Pdv.Modules.Catalog.Application.Commands.Suppliers;

/// <summary>
/// Command to delete a supplier (fornecedor).
/// </summary>
/// <param name="Id">The unique identifier of the supplier to delete.</param>
public sealed record DeleteSupplierCommand(Guid Id) : IRequest<Unit>;

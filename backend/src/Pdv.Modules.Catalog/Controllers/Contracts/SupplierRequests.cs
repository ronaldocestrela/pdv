namespace Pdv.Modules.Catalog.Controllers.Contracts;

/// <summary>
/// Payload to create a supplier.
/// </summary>
public sealed record CreateSupplierRequest(
    string Name,
    string? Document,
    string? Email,
    string? Phone,
    bool IsActive);

/// <summary>
/// Payload to update a supplier.
/// </summary>
public sealed record UpdateSupplierRequest(
    string Name,
    string? Document,
    string? Email,
    string? Phone,
    bool IsActive);

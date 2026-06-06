using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Catalog.Domain.Entities;

/// <summary>
/// Represents a supplier (fornecedor) who supplies products or services to the tenant.
/// </summary>
public sealed class Supplier : ITenantScoped
{
    /// <summary>
    /// Gets or sets the unique identifier of the supplier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier associated with the supplier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the name or corporate name (Razão Social / Nome Fantasia) of the supplier.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document number (e.g., CNPJ/CPF) of the supplier.
    /// </summary>
    public string? Document { get; set; }

    /// <summary>
    /// Gets or sets the email address of the supplier.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the contact phone number of the supplier.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the supplier is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

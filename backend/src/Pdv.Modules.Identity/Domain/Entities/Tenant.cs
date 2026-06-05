namespace Pdv.Modules.Identity.Domain.Entities;

/// <summary>
/// Representa um tenant (empresa/cliente) isolado no sistema.
/// Não implementa ITenantScoped pois é uma entidade cross-tenant gerenciada globalmente.
/// </summary>
public sealed class Tenant
{
    /// <summary>Identificador único do tenant.</summary>
    public int Id { get; set; }

    /// <summary>Nome da empresa ou organização.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Indica se o tenant está ativo e pode operar no sistema.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Data e hora UTC de criação do tenant.</summary>
    public DateTime CreatedAtUtc { get; set; }
}

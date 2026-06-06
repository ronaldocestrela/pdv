using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Application.Abstractions;

/// <summary>DTO de leitura de tenant para listagens administrativas.</summary>
public sealed record TenantAdminDto(Guid Id, string Name, bool IsActive, DateTime CreatedAtUtc);

/// <summary>
/// Contrato de acesso a dados da entidade Tenant, operando de forma cross-tenant
/// (sem filtro de isolamento por TenantId).
/// </summary>
public interface ITenantRepository
{
    /// <summary>Verifica se já existe um tenant com o nome informado.</summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Adiciona um novo tenant ao contexto.</summary>
    void Add(Tenant tenant);

    /// <summary>Retorna todos os tenants cadastrados.</summary>
    Task<IReadOnlyList<TenantAdminDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Retorna um tenant rastreável pelo ID, ou null se não encontrado.</summary>
    Task<Tenant?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Persiste todas as alterações pendentes no contexto.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}

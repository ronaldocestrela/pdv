using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Modules.Identity.Infrastructure.Persistence;

namespace Pdv.Modules.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de acesso à entidade <see cref="Tenant"/>, operando de forma cross-tenant
/// (sem filtro de isolamento por TenantId).
/// </summary>
public sealed class TenantRepository(IdentityDbContext db) : ITenantRepository
{
    private readonly IdentityDbContext _db = db;

    /// <summary>
    /// Verifica se já existe um tenant com o nome informado (busca global, sem filtro de tenant).
    /// </summary>
    public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
        _db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Name == name, ct);

    /// <summary>
    /// Adiciona um novo tenant ao contexto de persistência.
    /// </summary>
    public void Add(Tenant tenant) => _db.Tenants.Add(tenant);

    /// <summary>
    /// Retorna todos os tenants cadastrados, ordenados por nome, sem filtro de tenant.
    /// </summary>
    public async Task<IReadOnlyList<TenantAdminDto>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Tenants
            .IgnoreQueryFilters()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        return list.Select(t => new TenantAdminDto(t.Id, t.Name, t.IsActive, t.CreatedAtUtc)).ToList();
    }

    /// <summary>
    /// Retorna um tenant rastreável pelo ID para operações de escrita, ou null se não encontrado.
    /// </summary>
    public Task<Tenant?> GetTrackedByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Tenants.IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == id, ct);

    /// <summary>
    /// Persiste todas as alterações pendentes no contexto de dados.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}

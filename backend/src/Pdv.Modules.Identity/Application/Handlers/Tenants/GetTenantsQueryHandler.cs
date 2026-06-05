using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Queries.Tenants;

namespace Pdv.Modules.Identity.Application.Handlers.Tenants;

/// <summary>
/// Handler que retorna a lista de todos os tenants cadastrados (uso do Super Admin global).
/// </summary>
public sealed class GetTenantsQueryHandler(ITenantRepository tenants)
    : IRequestHandler<GetTenantsQuery, IReadOnlyList<TenantAdminDto>>
{
    private readonly ITenantRepository _tenants = tenants;

    /// <summary>
    /// Executa a query retornando todos os tenants ordenados por nome.
    /// </summary>
    public Task<IReadOnlyList<TenantAdminDto>> Handle(GetTenantsQuery request, CancellationToken cancellationToken) =>
        _tenants.GetAllAsync(cancellationToken);
}

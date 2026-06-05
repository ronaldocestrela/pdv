using MediatR;
using Microsoft.AspNetCore.Http;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Tenants;

namespace Pdv.Modules.Identity.Application.Handlers.Tenants;

/// <summary>
/// Handler para ativar ou desativar um tenant.
/// Lança <see cref="KeyNotFoundException"/> se o tenant não for encontrado.
/// </summary>
public sealed class SetTenantActiveHandler(ITenantRepository tenants) : IRequestHandler<SetTenantActiveCommand>
{
    private readonly ITenantRepository _tenants = tenants;

    /// <summary>
    /// Localiza o tenant e atualiza seu estado de ativação.
    /// </summary>
    public async Task Handle(SetTenantActiveCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenants.GetTrackedByIdAsync(request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant {request.TenantId} não encontrado.");

        tenant.IsActive = request.IsActive;
        await _tenants.SaveChangesAsync(cancellationToken);
    }
}

using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Tenants;

/// <summary>
/// Comando para ativar ou desativar um tenant pelo ID.
/// </summary>
/// <param name="TenantId">ID do tenant a ser modificado.</param>
/// <param name="IsActive">Novo estado de ativação desejado.</param>
public sealed record SetTenantActiveCommand(int TenantId, bool IsActive) : IRequest;

using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Tenants;

/// <summary>
/// Comando para criação de um novo tenant com seu usuário administrador inicial.
/// </summary>
/// <param name="Name">Nome da empresa/organização (deve ser único).</param>
/// <param name="AdminEmail">E-mail do primeiro usuário administrador do tenant.</param>
/// <param name="AdminPassword">Senha inicial do usuário administrador.</param>
public sealed record CreateTenantCommand(string Name, string AdminEmail, string AdminPassword)
    : IRequest<Guid>;

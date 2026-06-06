namespace Pdv.Modules.Identity.Application.Abstractions;

/// <summary>
/// Serviço responsável por executar o seed inicial de um tenant recém-criado:
/// garante a role Super Admin local e cria o usuário administrador inicial.
/// </summary>
public interface ITenantSeedService
{
    /// <summary>
    /// Executa o seed para o tenant especificado: cria a role Super Admin do tenant,
    /// vincula todas as permissões e cria o usuário admin inicial com a role atribuída.
    /// </summary>
    /// <param name="tenantId">ID do novo tenant.</param>
    /// <param name="adminEmail">E-mail do usuário administrador inicial.</param>
    /// <param name="adminPassword">Senha do usuário administrador inicial (em texto claro — será hasheada).</param>
    /// <param name="ct">Token de cancelamento.</param>
    Task SeedNewTenantAsync(Guid tenantId, string adminEmail, string adminPassword, CancellationToken ct = default);
}

using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Tenants;
using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Application.Handlers.Tenants;

/// <summary>
/// Handler responsável por criar um novo tenant com seu usuário administrador inicial.
/// Valida nome único e e-mail, persiste o tenant e executa o seed interno via
/// <see cref="ITenantSeedService"/>.
/// </summary>
public sealed class CreateTenantHandler(
    ITenantRepository tenants,
    IUserAdminRepository users,
    ITenantSeedService tenantSeedService) : IRequestHandler<CreateTenantCommand, int>
{
    private readonly ITenantRepository _tenants = tenants;
    private readonly IUserAdminRepository _users = users;
    private readonly ITenantSeedService _tenantSeedService = tenantSeedService;

    /// <summary>
    /// Executa a criação do tenant: valida unicidade de nome e e-mail, persiste o tenant
    /// e delega a criação do admin inicial ao <see cref="ITenantSeedService"/>.
    /// </summary>
    public async Task<int> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        var email = request.AdminEmail.Trim();

        if (await _tenants.ExistsByNameAsync(name, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(CreateTenantCommand.Name), "Já existe um tenant com este nome.")]);

        if (await _users.EmailExistsAsync(email, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(CreateTenantCommand.AdminEmail), "E-mail já cadastrado no sistema.")]);

        var tenant = new Tenant
        {
            Name = name,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _tenants.Add(tenant);
        await _tenants.SaveChangesAsync(cancellationToken);

        await _tenantSeedService.SeedNewTenantAsync(tenant.Id, email, request.AdminPassword, cancellationToken);

        return tenant.Id;
    }
}

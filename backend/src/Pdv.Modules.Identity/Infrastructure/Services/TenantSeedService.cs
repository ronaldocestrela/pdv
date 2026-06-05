using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Modules.Identity.Infrastructure.Persistence;
using Pdv.Shared.Kernel.Security;

namespace Pdv.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Executa o seed inicial de um novo tenant: cria a role Super Admin local,
/// vincula todas as permissões e cria o usuário administrador com a role atribuída.
/// </summary>
public sealed class TenantSeedService(IdentityDbContext db, IPasswordHasher passwordHasher) : ITenantSeedService
{
    private readonly IdentityDbContext _db = db;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Garante role Super Admin para o tenant, vincula todas as permissões e
    /// cria o usuário administrador inicial com e-mail e senha fornecidos.
    /// </summary>
    public async Task SeedNewTenantAsync(int tenantId, string adminEmail, string adminPassword, CancellationToken ct = default)
    {
        // 1. Garante a role Super Admin para o novo tenant
        var superAdminRole = await _db.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.TenantId == tenantId && r.Name == KnownRoles.SuperAdmin, ct);

        if (superAdminRole is null)
        {
            superAdminRole = new Role { TenantId = tenantId, Name = KnownRoles.SuperAdmin };
            _db.Roles.Add(superAdminRole);
            await _db.SaveChangesAsync(ct);
        }

        // 2. Vincula todas as permissões à role Super Admin do tenant
        var linkedPermissionIds = (await _db.RolePermissions
            .Where(rp => rp.RoleId == superAdminRole.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct))
            .ToHashSet();

        var allPermissionIds = await _db.Permissions
            .AsNoTracking()
            .Select(p => p.Id)
            .ToListAsync(ct);

        foreach (var pid in allPermissionIds)
        {
            if (linkedPermissionIds.Contains(pid))
                continue;
            _db.RolePermissions.Add(new RolePermission { RoleId = superAdminRole.Id, PermissionId = pid });
            linkedPermissionIds.Add(pid);
        }

        await _db.SaveChangesAsync(ct);

        // 3. Cria o usuário administrador inicial do tenant
        var email = adminEmail.Trim();
        var existingUser = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email, ct);

        if (existingUser is null)
        {
            var adminUser = new User
            {
                TenantId = tenantId,
                Email = email,
                PasswordHash = _passwordHasher.Hash(adminPassword),
                IsActive = true,
            };
            _db.Users.Add(adminUser);
            await _db.SaveChangesAsync(ct);

            _db.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = superAdminRole.Id });
            await _db.SaveChangesAsync(ct);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Shared.Kernel.Security;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Modules.Identity.Infrastructure.Persistence;

namespace Pdv.Modules.Identity.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    private const int DefaultTenantId = 1;

    public static async Task ApplyAsync(
        IdentityDbContext db,
        SeedOptions options,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        await EnsurePermissionsAsync(db, logger, cancellationToken);
        await EnsureRolesAndLinksAsync(db, logger, cancellationToken);
        await EnsureSuperAdminAsync(db, options, passwordHasher, logger, cancellationToken);
    }

    private static async Task EnsurePermissionsAsync(IdentityDbContext db, ILogger logger, CancellationToken ct)
    {
        var existing = await db.Permissions.Select(p => p.Name).ToListAsync(ct);
        var set = existing.ToHashSet(StringComparer.Ordinal);
        var added = 0;
        foreach (var name in KnownPermissions.All.Distinct(StringComparer.Ordinal))
        {
            if (set.Contains(name))
                continue;
            db.Permissions.Add(new Permission { Name = name });
            set.Add(name);
            added++;
        }

        if (added > 0)
        {
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Added {Count} permission(s).", added);
        }
    }

    private static async Task EnsureRolesAndLinksAsync(IdentityDbContext db, ILogger logger, CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(
            r => r.TenantId == DefaultTenantId && r.Name == KnownRoles.SuperAdmin,
            ct);
        if (role is null)
        {
            role = new Role { TenantId = DefaultTenantId, Name = KnownRoles.SuperAdmin };
            db.Roles.Add(role);
            await db.SaveChangesAsync(ct);
        }

        var linkedIds = (await db.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(ct))
            .ToHashSet();

        var allPermissionIds = await db.Permissions.AsNoTracking().Select(p => p.Id).ToListAsync(ct);

        foreach (var pid in allPermissionIds)
        {
            if (linkedIds.Contains(pid))
                continue;

            db.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = pid });
            linkedIds.Add(pid);
        }

        await db.SaveChangesAsync(ct);

        var totalLinks = await db.RolePermissions.CountAsync(r => r.RoleId == role.Id, ct);
        logger.LogInformation("Role '{Role}' linked to {Count} permissions.", KnownRoles.SuperAdmin, totalLinks);
    }

    private static async Task EnsureSuperAdminAsync(
        IdentityDbContext db,
        SeedOptions options,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken ct)
    {
        var email = options.SuperAdminEmail.Trim();

        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.TenantId == DefaultTenantId && u.Email == email, ct);

        var superRole = await db.Roles.AsNoTracking()
            .FirstAsync(r => r.TenantId == DefaultTenantId && r.Name == KnownRoles.SuperAdmin, ct);

        if (user is null)
        {
            user = new User
            {
                TenantId = DefaultTenantId,
                Email = email,
                PasswordHash = passwordHasher.Hash(options.SuperAdminPassword),
                IsActive = true,
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = superRole.Id });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Created Super Admin user {Email}", email);
            return;
        }

        if (user.UserRoles.All(ur => ur.Role.Name != KnownRoles.SuperAdmin))
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = superRole.Id });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Linked Super Admin role to {Email}", email);
        }
    }
}

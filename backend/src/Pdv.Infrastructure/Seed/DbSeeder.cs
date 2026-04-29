using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Seed;

/// <summary>Seeds baseline permissions, Super Admin role/user (idempotent for dev).</summary>
public static class DbSeeder
{
    public static async Task ApplyAsync(AppDbContext db, SeedOptions options, IPasswordHasher passwordHasher, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await db.Permissions.AnyAsync(cancellationToken) is false)
        {
            foreach (var name in PermissionCatalog.All.Distinct(StringComparer.Ordinal))
                db.Permissions.Add(new Permission { Name = name });
            await db.SaveChangesAsync(cancellationToken);
        }

        await EnsureRolesAndLinksAsync(db, logger, cancellationToken);
        await EnsureSuperAdminAsync(db, options, passwordHasher, logger, cancellationToken);
    }

    private static async Task EnsureRolesAndLinksAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.SuperAdmin, ct);
        if (role is null)
        {
            role = new Role { Name = RoleNames.SuperAdmin };
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
        logger.LogInformation("Role '{Role}' linked to {Count} permissions.", RoleNames.SuperAdmin, totalLinks);
    }

    private static async Task EnsureSuperAdminAsync(
        AppDbContext db,
        SeedOptions options,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken ct)
    {
        var email = options.SuperAdminEmail.Trim();

        var user = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        var superRole = await db.Roles.AsNoTracking().FirstAsync(r => r.Name == RoleNames.SuperAdmin, ct);

        if (user is null)
        {
            user = new User
            {
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

        if (user.UserRoles.All(ur => ur.Role.Name != RoleNames.SuperAdmin))
        {
            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = superRole.Id });
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Linked Super Admin role to {Email}", email);
        }
    }
}

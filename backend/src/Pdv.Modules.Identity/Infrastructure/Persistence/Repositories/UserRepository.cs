using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Modules.Identity.Infrastructure.Persistence;

namespace Pdv.Modules.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="UserRepository"/> class.
/// </summary>
public sealed class UserRepository(IdentityDbContext db) : IUserRepository
{
    private readonly IdentityDbContext _db = db;

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<User?> GetWithPermissionsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return BaseQueryWithPermissions().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<User?> GetWithPermissionsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return BaseQueryWithPermissions().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<User?> GetWithPermissionsByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return BaseQueryWithPermissions()
            .FirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken,
                cancellationToken);
    }

    /// <summary>
    /// Persists all tracked changes in this database context.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);

    private IQueryable<User> BaseQueryWithPermissions()
    {
        return _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .AsSplitQuery();
    }
}

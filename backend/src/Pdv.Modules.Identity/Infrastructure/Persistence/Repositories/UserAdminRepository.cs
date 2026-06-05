using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Modules.Identity.Infrastructure.Persistence;

namespace Pdv.Modules.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="UserAdminRepository"/> class.
/// </summary>
public sealed class UserAdminRepository(IdentityDbContext db) : IUserAdminRepository
{
    private readonly IdentityDbContext _db = db;

    /// <summary>
    /// Retrieves a list of DTO summaries.
    /// </summary>
    public async Task<IReadOnlyList<UserAdminDto>> ListUsersWithRolesAsync(CancellationToken cancellationToken = default)
    {
        var users = await _db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        return users
            .Select(u => new UserAdminDto(
                u.Id,
                u.Email,
                u.IsActive,
                u.UserRoles.Select(ur => ur.RoleId).OrderBy(id => id).ToList()))
            .ToList();
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<User?> GetTrackedWithRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Executes the AllRoleIdsExistAsync operation.
    /// </summary>
    public async Task<bool> AllRoleIdsExistAsync(IReadOnlyList<int> roleIds, CancellationToken cancellationToken = default)
    {
        if (roleIds.Count == 0)
            return true;
        var distinct = roleIds.Distinct().ToList();
        var count = await _db.Roles.CountAsync(r => distinct.Contains(r.Id), cancellationToken);
        return count == distinct.Count;
    }

    /// <summary>
    /// Executes the EmailExistsAsync operation.
    /// </summary>
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void Add(User user) => _db.Users.Add(user);

    /// <summary>
    /// Persists all tracked changes in this database context.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}

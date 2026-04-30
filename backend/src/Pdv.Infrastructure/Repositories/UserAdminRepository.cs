using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

public sealed class UserAdminRepository : IUserAdminRepository
{
    private readonly AppDbContext _db;

    public UserAdminRepository(AppDbContext db)
    {
        _db = db;
    }

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

    public Task<User?> GetTrackedWithRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<bool> AllRoleIdsExistAsync(IReadOnlyList<int> roleIds, CancellationToken cancellationToken = default)
    {
        if (roleIds.Count == 0)
            return true;
        var distinct = roleIds.Distinct().ToList();
        var count = await _db.Roles.CountAsync(r => distinct.Contains(r.Id), cancellationToken);
        return count == distinct.Count;
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) =>
        _db.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public void Add(User user) => _db.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}

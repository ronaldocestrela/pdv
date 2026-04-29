using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetWithPermissionsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return BaseQueryWithPermissions().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<User?> GetWithPermissionsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return BaseQueryWithPermissions().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetWithPermissionsByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return BaseQueryWithPermissions()
            .FirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken,
                cancellationToken);
    }

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

using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetWithPermissionsByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetWithPermissionsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetWithPermissionsByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

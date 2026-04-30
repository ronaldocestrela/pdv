using Pdv.Domain.Entities;

namespace Pdv.Application.Abstractions;

public sealed record UserAdminDto(int Id, string Email, bool IsActive, IReadOnlyList<int> RoleIds);

public interface IUserAdminRepository
{
    Task<IReadOnlyList<UserAdminDto>> ListUsersWithRolesAsync(CancellationToken cancellationToken = default);

    Task<User?> GetTrackedWithRolesAsync(int userId, CancellationToken cancellationToken = default);

    Task<bool> AllRoleIdsExistAsync(IReadOnlyList<int> roleIds, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

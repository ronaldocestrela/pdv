using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Application.Abstractions;

public sealed record UserAdminDto(Guid Id, string Email, bool IsActive, IReadOnlyList<Guid> RoleIds);

public interface IUserAdminRepository
{
    Task<IReadOnlyList<UserAdminDto>> ListUsersWithRolesAsync(CancellationToken cancellationToken = default);

    Task<User?> GetTrackedWithRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> AllRoleIdsExistAsync(IReadOnlyList<Guid> roleIds, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

using Pdv.Domain.Entities;

namespace Pdv.Application.Abstractions;

public sealed record RoleAdminDto(int Id, string Name, IReadOnlyList<string> Permissions);

public interface IRoleRepository
{
    Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default);

    Task<RoleAdminDto?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Role?> GetTrackedWithPermissionsAsync(int id, CancellationToken cancellationToken = default);

    Task<Role?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default);

    Task SetRolePermissionsByNamesAsync(int roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, int? excludeRoleId, CancellationToken cancellationToken = default);

    void Add(Role role);

    void Remove(Role role);

    Task<IReadOnlyList<string>> ListPermissionNamesAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

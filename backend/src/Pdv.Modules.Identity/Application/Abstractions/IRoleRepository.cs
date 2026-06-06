using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Application.Abstractions;

public sealed record RoleAdminDto(Guid Id, string Name, IReadOnlyList<string> Permissions);

public interface IRoleRepository
{
    Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default);

    Task<RoleAdminDto?> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Role?> GetTrackedWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Role?> GetTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SetRolePermissionsByNamesAsync(Guid roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, Guid? excludeRoleId, CancellationToken cancellationToken = default);

    void Add(Role role);

    void Remove(Role role);

    Task<IReadOnlyList<string>> ListPermissionNamesAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

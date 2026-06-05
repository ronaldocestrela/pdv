using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Identity.Domain.Entities;

public sealed class Role : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

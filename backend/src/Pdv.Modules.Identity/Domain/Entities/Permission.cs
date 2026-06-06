namespace Pdv.Modules.Identity.Domain.Entities;

public sealed class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

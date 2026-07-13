namespace Pdv.Web.Models;

/// <summary>Espelho de RoleAdminDto.</summary>
public class RoleAdminDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string> Permissions { get; set; } = [];
}

/// <summary>Espelho de UserAdminDto.</summary>
public class UserAdminDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsActive { get; set; }
    public List<string> RoleIds { get; set; } = [];
}

/// <summary>Espelho de TenantAdminDto.</summary>
public class TenantAdminDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; }
}

/// <summary>Payload de POST /api/tenants/register.</summary>
public class RegisterTenantPayload
{
    public string Name { get; set; } = "";
    public string AdminEmail { get; set; } = "";
    public string AdminPassword { get; set; } = "";
}

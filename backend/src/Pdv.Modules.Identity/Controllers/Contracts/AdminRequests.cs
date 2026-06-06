namespace Pdv.Modules.Identity.Controllers.Contracts;

public sealed record CreateRoleRequest(string Name);

public sealed record UpdateRoleRequest(string Name);

public sealed record SetRolePermissionsRequest(IReadOnlyList<string> PermissionNames);

public sealed record SetUserRolesRequest(IReadOnlyList<Guid> RoleIds);

public sealed record CreateUserRequest(string Email, string Password, bool IsActive = true);

/// <summary>Dados para registro ou criação de um novo tenant com seu administrador inicial.</summary>
public sealed record RegisterTenantRequest(string Name, string AdminEmail, string AdminPassword);

/// <summary>Altera o estado de ativação de um tenant.</summary>
public sealed record SetTenantActiveRequest(bool IsActive);


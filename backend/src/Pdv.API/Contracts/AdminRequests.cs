namespace Pdv.API.Contracts;

public sealed record CreateRoleRequest(string Name);

public sealed record UpdateRoleRequest(string Name);

public sealed record SetRolePermissionsRequest(IReadOnlyList<string> PermissionNames);

public sealed record SetUserRolesRequest(IReadOnlyList<int> RoleIds);

public sealed record CreateUserRequest(string Email, string Password, bool IsActive = true);

public sealed record IdResponse(int Id);

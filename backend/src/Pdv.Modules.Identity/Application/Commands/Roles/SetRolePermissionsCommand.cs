using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Roles;

public sealed record SetRolePermissionsCommand(int RoleId, IReadOnlyList<string> PermissionNames) : IRequest<Unit>;

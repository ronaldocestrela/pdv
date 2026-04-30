using MediatR;

namespace Pdv.Application.Commands.Roles;

public sealed record SetRolePermissionsCommand(int RoleId, IReadOnlyList<string> PermissionNames) : IRequest<Unit>;

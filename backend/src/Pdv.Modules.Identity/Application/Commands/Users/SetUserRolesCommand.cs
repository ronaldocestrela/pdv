using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Users;

public sealed record SetUserRolesCommand(int UserId, IReadOnlyList<int> RoleIds) : IRequest<Unit>;

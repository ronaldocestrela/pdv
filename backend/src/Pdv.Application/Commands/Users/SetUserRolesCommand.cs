using MediatR;

namespace Pdv.Application.Commands.Users;

public sealed record SetUserRolesCommand(int UserId, IReadOnlyList<int> RoleIds) : IRequest<Unit>;

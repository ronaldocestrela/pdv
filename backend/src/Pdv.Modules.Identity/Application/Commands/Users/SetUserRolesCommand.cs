using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Users;

public sealed record SetUserRolesCommand(Guid UserId, IReadOnlyList<Guid> RoleIds) : IRequest<Unit>;

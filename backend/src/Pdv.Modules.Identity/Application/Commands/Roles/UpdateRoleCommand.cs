using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Roles;

public sealed record UpdateRoleCommand(Guid Id, string Name) : IRequest<Unit>;

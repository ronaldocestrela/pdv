using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Roles;

public sealed record UpdateRoleCommand(int Id, string Name) : IRequest<Unit>;

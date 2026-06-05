using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Roles;

public sealed record DeleteRoleCommand(int Id) : IRequest<Unit>;

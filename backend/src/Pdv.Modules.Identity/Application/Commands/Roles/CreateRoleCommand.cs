using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Roles;

public sealed record CreateRoleCommand(string Name) : IRequest<int>;

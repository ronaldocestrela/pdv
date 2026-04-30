using MediatR;

namespace Pdv.Application.Commands.Roles;

public sealed record CreateRoleCommand(string Name) : IRequest<int>;

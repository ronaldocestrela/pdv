using MediatR;

namespace Pdv.Application.Commands.Roles;

public sealed record UpdateRoleCommand(int Id, string Name) : IRequest<Unit>;

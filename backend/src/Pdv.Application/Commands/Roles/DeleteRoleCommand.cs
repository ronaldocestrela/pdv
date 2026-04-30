using MediatR;

namespace Pdv.Application.Commands.Roles;

public sealed record DeleteRoleCommand(int Id) : IRequest<Unit>;

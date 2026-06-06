using MediatR;

namespace Pdv.Modules.Identity.Application.Commands.Users;

public sealed record CreateUserCommand(string Email, string Password, bool IsActive = true) : IRequest<Guid>;

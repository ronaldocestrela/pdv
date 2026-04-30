using MediatR;

namespace Pdv.Application.Commands.Users;

public sealed record CreateUserCommand(string Email, string Password, bool IsActive = true) : IRequest<int>;

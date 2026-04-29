using MediatR;
using Pdv.Application.Auth;

namespace Pdv.Application.Commands.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<TokenResponseDto?>;

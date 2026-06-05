using MediatR;
using Pdv.Modules.Identity.Application.Auth;

namespace Pdv.Modules.Identity.Application.Commands.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<TokenResponseDto?>;

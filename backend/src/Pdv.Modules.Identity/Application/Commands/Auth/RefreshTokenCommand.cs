using MediatR;
using Pdv.Modules.Identity.Application.Auth;

namespace Pdv.Modules.Identity.Application.Commands.Auth;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponseDto?>;

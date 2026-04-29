using MediatR;
using Pdv.Application.Auth;

namespace Pdv.Application.Commands.Auth;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<TokenResponseDto?>;

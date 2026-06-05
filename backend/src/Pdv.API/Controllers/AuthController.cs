using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Auth;
using Pdv.Application.Commands.Auth;

namespace Pdv.API.Controllers;

/// <summary>
/// Initializes a new instance of the <see cref="AuthController"/> class.
/// </summary>
/// <param name="mediator">The mediator sender instance.</param>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Authenticates a user with credentials and returns JWT and refresh tokens.
    /// </summary>
    /// <param name="request">The login request containing email and password.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An action result containing the token response DTO if successful; otherwise, Unauthorized.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        if (result is null)
            return Unauthorized();
        return Ok(result);
    }

    /// <summary>
    /// Refreshes an expired JWT access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh request containing the refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An action result containing the new token response DTO if successful; otherwise, Unauthorized.</returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), cancellationToken);
        if (result is null)
            return Unauthorized();
        return Ok(result);
    }
}

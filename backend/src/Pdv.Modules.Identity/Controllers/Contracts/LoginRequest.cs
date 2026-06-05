namespace Pdv.Modules.Identity.Controllers.Contracts;

public sealed class LoginRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

namespace Pdv.Modules.Identity.Controllers.Contracts;

public sealed class RefreshRequest
{
    public required string RefreshToken { get; init; }
}

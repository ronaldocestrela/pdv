namespace Pdv.API.Contracts;

public sealed class RefreshRequest
{
    public required string RefreshToken { get; init; }
}

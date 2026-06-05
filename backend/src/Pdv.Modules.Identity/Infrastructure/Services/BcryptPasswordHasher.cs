using Pdv.Modules.Identity.Application.Abstractions;

namespace Pdv.Modules.Identity.Infrastructure.Services;

public sealed class BcryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Executes the Hash operation.
    /// </summary>
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    /// <summary>
    /// Executes the Verify operation.
    /// </summary>
    public bool Verify(string password, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(password, passwordHash);
}

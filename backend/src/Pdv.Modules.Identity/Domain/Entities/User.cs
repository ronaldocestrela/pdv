using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Modules.Identity.Domain.Entities;

public sealed class User : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; } = 1;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

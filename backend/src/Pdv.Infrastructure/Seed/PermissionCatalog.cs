using Pdv.Application.Security;

namespace Pdv.Infrastructure.Seed;

internal static class PermissionCatalog
{
    public static readonly IReadOnlyList<string> All = KnownPermissions.All;
}

internal static class RoleNames
{
    public const string SuperAdmin = "Super Admin";
}

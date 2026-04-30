using Pdv.Application.Security;

namespace Pdv.Infrastructure.Seed;

internal static class PermissionCatalog
{
    public static readonly IReadOnlyList<string> All = KnownPermissions.All;
}


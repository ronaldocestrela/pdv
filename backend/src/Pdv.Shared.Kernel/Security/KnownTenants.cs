using System;

namespace Pdv.Shared.Kernel.Security;

/// <summary>Well-known tenant configuration and IDs.</summary>
public static class KnownTenants
{
    /// <summary>
    /// The default Host Tenant ID.
    /// </summary>
    public static readonly Guid HostTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
}

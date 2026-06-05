using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Shared.Kernel.Services;

public sealed class SystemTenantContext : ITenantContext
{
    public int? TenantId => null;

    public bool IsSuperAdmin => true;
}

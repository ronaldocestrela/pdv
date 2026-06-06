using Pdv.Shared.Kernel.Abstractions;

namespace Pdv.Shared.Kernel.Services;

public sealed class SystemTenantContext : ITenantContext
{
    public Guid? TenantId => null;

    public bool IsSuperAdmin => true;
}

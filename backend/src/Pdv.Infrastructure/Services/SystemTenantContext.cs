using Pdv.Application.Abstractions;

namespace Pdv.Infrastructure.Services;

public sealed class SystemTenantContext : ITenantContext
{
    public int? TenantId => null;

    public bool IsSuperAdmin => true;
}

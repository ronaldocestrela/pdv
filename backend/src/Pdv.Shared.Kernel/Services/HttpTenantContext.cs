using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Pdv.Shared.Kernel.Abstractions;
using Pdv.Shared.Kernel.Security;

namespace Pdv.Shared.Kernel.Services;

/// <summary>
/// HttpTenantContext extracts the tenant scope details from the active HttpContext User claims.
/// </summary>
public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid? TenantId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var tenantId) ? tenantId : null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("is_super_admin");
            return bool.TryParse(value, out var isSuperAdmin) && isSuperAdmin;
        }
    }
}

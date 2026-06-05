using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Pdv.Application.Abstractions;
using Pdv.Application.Security;

namespace Pdv.Infrastructure.Services;

public sealed class HttpTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? TenantId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("tenant_id");
            return int.TryParse(value, out var tenantId) ? tenantId : null;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("is_super_admin");
            if (bool.TryParse(value, out var isSuperAdmin) && isSuperAdmin)
                return true;

            return _httpContextAccessor.HttpContext?.User.HasClaim("permission", KnownPermissions.RoleManage) == true
                && _httpContextAccessor.HttpContext?.User.HasClaim("permission", KnownPermissions.UserManage) == true;
        }
    }
}

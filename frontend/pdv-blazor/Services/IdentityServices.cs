using System.Net.Http.Json;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/roles.ts.</summary>
public class RolesService(HttpClient http)
{
    public virtual async Task<List<RoleAdminDto>> ListRolesAsync()
        => await http.GetFromJsonAsync<List<RoleAdminDto>>("/api/roles") ?? [];

    public virtual async Task<RoleAdminDto> GetRoleAsync(string id)
        => await http.GetFromJsonAsync<RoleAdminDto>($"/api/roles/{id}") ?? throw new();

    public virtual async Task<string> CreateRoleAsync(string name)
    {
        var res = await http.PostAsJsonAsync("/api/roles", new { name });
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<IdResult3>() ?? throw new();
        return data.Id;
    }

    public virtual async Task UpdateRoleAsync(string id, string name)
        => (await http.PutAsJsonAsync($"/api/roles/{id}", new { name })).EnsureSuccessStatusCode();

    public virtual async Task DeleteRoleAsync(string id)
        => (await http.DeleteAsync($"/api/roles/{id}")).EnsureSuccessStatusCode();

    public virtual async Task SetRolePermissionsAsync(string id, List<string> permissionNames)
        => (await http.PutAsJsonAsync($"/api/roles/{id}/permissions", new { permissionNames }))
           .EnsureSuccessStatusCode();
}

/// <summary>Equivalente a services/users.ts.</summary>
public class UsersService(HttpClient http)
{
    public virtual async Task<List<UserAdminDto>> ListUsersAsync()
        => await http.GetFromJsonAsync<List<UserAdminDto>>("/api/users") ?? [];

    public virtual async Task<string> CreateUserAsync(string email, string password, bool isActive = true)
    {
        var res = await http.PostAsJsonAsync("/api/users", new { email, password, isActive });
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<IdResult3>() ?? throw new();
        return data.Id;
    }

    public virtual async Task SetUserRolesAsync(string userId, List<string> roleIds)
        => (await http.PutAsJsonAsync($"/api/users/{userId}/roles", new { roleIds }))
           .EnsureSuccessStatusCode();
}

/// <summary>Equivalente a services/tenants.ts.</summary>
public class TenantsService(HttpClient http)
{
    public virtual async Task<string> RegisterTenantAsync(RegisterTenantPayload payload)
    {
        var res = await http.PostAsJsonAsync("/api/tenants/register", payload);
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<IdResult3>() ?? throw new();
        return data.Id;
    }

    public virtual async Task<List<TenantAdminDto>> ListTenantsAsync()
        => await http.GetFromJsonAsync<List<TenantAdminDto>>("/api/tenants") ?? [];

    public virtual async Task SetTenantActiveAsync(string tenantId, bool isActive)
        => (await http.PutAsJsonAsync($"/api/tenants/{tenantId}/activate", new { isActive }))
           .EnsureSuccessStatusCode();
}

internal record IdResult3(string Id);

using System.Net.Http.Json;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/permissions.ts.</summary>
public class PermissionsService(HttpClient http)
{
    public async Task<List<string>> GetPermissionCatalogAsync()
        => await http.GetFromJsonAsync<List<string>>("/api/permissions") ?? [];
}

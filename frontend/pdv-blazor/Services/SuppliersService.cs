using System.Net.Http.Json;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/suppliers.ts.</summary>
public class SuppliersService(HttpClient http)
{
    public virtual async Task<List<SupplierSummaryDto>> ListSuppliersAsync()
        => await http.GetFromJsonAsync<List<SupplierSummaryDto>>("/api/suppliers") ?? [];

    public virtual async Task<SupplierSummaryDto?> GetSupplierAsync(string id)
    {
        var res = await http.GetAsync($"/api/suppliers/{id}");
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<SupplierSummaryDto>();
    }

    public virtual async Task<string> CreateSupplierAsync(string name, string? document, string? email, string? phone, bool isActive)
    {
        var res = await http.PostAsJsonAsync("/api/suppliers", new { name, document, email, phone, isActive });
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<IdResult2>() ?? throw new();
        return data.Id;
    }

    public virtual async Task UpdateSupplierAsync(string id, string name, string? document, string? email, string? phone, bool isActive)
    {
        var res = await http.PutAsJsonAsync($"/api/suppliers/{id}", new { name, document, email, phone, isActive });
        res.EnsureSuccessStatusCode();
    }

    public virtual async Task DeleteSupplierAsync(string id)
        => (await http.DeleteAsync($"/api/suppliers/{id}")).EnsureSuccessStatusCode();
}

internal record IdResult2(string Id);

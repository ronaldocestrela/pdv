using System.Net.Http.Json;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/products.ts.</summary>
public class ProductsService(HttpClient http)
{
    public virtual async Task<List<ProductSummaryDto>> ListProductsAsync()
    {
        return await http.GetFromJsonAsync<List<ProductSummaryDto>>("/api/products") ?? [];
    }

    public virtual async Task<ProductDetailDto?> GetProductAsync(string id)
    {
        var res = await http.GetAsync($"/api/products/{id}");
        if (res.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<ProductDetailDto>();
    }

    public virtual async Task<string> CreateProductAsync(string name, bool isActive)
    {
        var res = await http.PostAsJsonAsync("/api/products", new { name, isActive });
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<IdResult>() ?? throw new();
        return data.Id;
    }

    public virtual async Task UpdateProductAsync(string id, string name, bool isActive)
    {
        var res = await http.PutAsJsonAsync($"/api/products/{id}", new { name, isActive });
        res.EnsureSuccessStatusCode();
    }

    public virtual async Task DeleteProductAsync(string id)
    {
        var res = await http.DeleteAsync($"/api/products/{id}");
        res.EnsureSuccessStatusCode();
    }

    public virtual async Task<string> CreateVariationAsync(string productId, string name, string? barcode, int stockQuantity, decimal unitPrice)
    {
        var res = await http.PostAsJsonAsync("/api/variations", new { productId, name, barcode, stockQuantity, unitPrice });
        res.EnsureSuccessStatusCode();
        var data = await res.Content.ReadFromJsonAsync<IdResult>() ?? throw new();
        return data.Id;
    }

    public virtual async Task UpdateVariationAsync(string id, string name, string? barcode, int stockQuantity, decimal unitPrice)
    {
        var res = await http.PutAsJsonAsync($"/api/variations/{id}", new { name, barcode, stockQuantity, unitPrice });
        res.EnsureSuccessStatusCode();
    }

    public virtual async Task DeleteVariationAsync(string id)
    {
        var res = await http.DeleteAsync($"/api/variations/{id}");
        res.EnsureSuccessStatusCode();
    }
}

internal record IdResult(string Id);

using System.Net.Http.Json;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/stock.ts.</summary>
public class StockService(HttpClient http)
{
    public virtual async Task AdjustStockAsync(string productVariationId, int quantity, string? reason)
    {
        var res = await http.PostAsJsonAsync("/api/stock/adjust",
            new { productVariationId, quantity, reason });
        res.EnsureSuccessStatusCode();
    }

    public virtual async Task<List<StockMovementDto>> ListMovementsAsync(string? variationId = null, int take = 100)
    {
        var url = $"/api/stock/movements?take={take}";
        if (!string.IsNullOrEmpty(variationId)) url += $"&variationId={variationId}";
        return await http.GetFromJsonAsync<List<StockMovementDto>>(url) ?? [];
    }
}

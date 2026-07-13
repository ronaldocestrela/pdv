using System.Net.Http.Json;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/sales.ts.</summary>
public class SalesService(HttpClient http)
{
    public async Task<CreateSaleResultDto> CreateSaleAsync(CreateSalePayload payload)
    {
        var res = await http.PostAsJsonAsync("/api/sales", payload);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<CreateSaleResultDto>() ?? throw new();
    }

    public async Task<List<SaleListItemDto>> ListSalesAsync(int take = 50)
    {
        return await http.GetFromJsonAsync<List<SaleListItemDto>>($"/api/sales?take={take}") ?? [];
    }
}

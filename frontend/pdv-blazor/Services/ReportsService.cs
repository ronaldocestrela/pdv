using System.Net.Http.Json;
using Pdv.Web.Models;

namespace Pdv.Web.Services;

/// <summary>Equivalente a services/reports.ts.</summary>
public class ReportsService(HttpClient http)
{
    public async Task<SalesReportDto> GetSalesReportAsync(string fromUtc, string toUtc)
        => await http.GetFromJsonAsync<SalesReportDto>(
               $"/api/reports/sales?fromUtc={Uri.EscapeDataString(fromUtc)}&toUtc={Uri.EscapeDataString(toUtc)}")
           ?? new();

    public async Task<List<TopProductReportDto>> GetTopProductsAsync(string fromUtc, string toUtc, int take = 20)
        => await http.GetFromJsonAsync<List<TopProductReportDto>>(
               $"/api/reports/top-products?fromUtc={Uri.EscapeDataString(fromUtc)}&toUtc={Uri.EscapeDataString(toUtc)}&take={take}")
           ?? [];

    public async Task<List<CashFlowReportRowDto>> GetCashFlowAsync(string fromUtc, string toUtc, int take = 100)
        => await http.GetFromJsonAsync<List<CashFlowReportRowDto>>(
               $"/api/reports/cashflow?fromUtc={Uri.EscapeDataString(fromUtc)}&toUtc={Uri.EscapeDataString(toUtc)}&take={take}")
           ?? [];

    public async Task<List<StockReportRowDto>> GetStockReportAsync(int take = 500)
        => await http.GetFromJsonAsync<List<StockReportRowDto>>($"/api/reports/stock?take={take}") ?? [];
}

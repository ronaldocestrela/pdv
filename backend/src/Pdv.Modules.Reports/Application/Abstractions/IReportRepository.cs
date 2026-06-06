using Pdv.Shared.Kernel.Enums;

namespace Pdv.Modules.Reports.Application.Abstractions;

public sealed record SalesReportDto(int SaleCount, decimal TotalAmount);

public sealed record TopProductReportDto(
    Guid ProductVariationId,
    string ProductName,
    string VariationName,
    int QuantitySold,
    decimal Revenue);

public sealed record CashFlowReportRowDto(
    Guid Id,
    CashFlowType Type,
    decimal Amount,
    string Description,
    DateTime CreatedAtUtc,
    Guid? SaleId);

public sealed record StockReportRowDto(
    Guid ProductVariationId,
    string ProductName,
    string VariationName,
    int StockQuantity);

public interface IReportRepository
{
    Task<SalesReportDto> GetSalesSummaryAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TopProductReportDto>> GetTopProductsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CashFlowReportRowDto>> ListCashFlowsAsync(
        DateTime fromUtc,
        DateTime toUtc,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StockReportRowDto>> ListStockLevelsAsync(int take, CancellationToken cancellationToken = default);
}

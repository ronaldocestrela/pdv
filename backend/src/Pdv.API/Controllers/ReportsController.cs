using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

/// <summary>
/// Controller responsável pela geração de relatórios gerenciais e operacionais (Vendas, Produtos mais vendidos, Fluxo de Caixa e Estoque).
/// </summary>
/// <remarks>
/// Inicializa uma nova instância da classe <see cref="ReportsController"/>.
/// </remarks>
/// <param name="mediator">Instância do remetente do MediatR para processamento de CQRS.</param>
[ApiController]
[Route("api/reports")]
public sealed class ReportsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Gera o relatório consolidado de vendas em um período específico.
    /// </summary>
    /// <param name="fromUtc">Data inicial em UTC.</param>
    /// <param name="toUtc">Data final em UTC.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Um DTO contendo a contagem e o valor total acumulado das vendas.</returns>
    [HttpGet("sales")]
    [Authorize(Policy = KnownPermissions.ReportView)]
    [ProducesResponseType(typeof(SalesReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sales(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(new GetSalesReportQuery(fromUtc, toUtc), cancellationToken);
        return Ok(dto);
    }

    /// <summary>
    /// Gera o relatório dos produtos mais vendidos em um determinado período.
    /// </summary>
    /// <param name="fromUtc">Data inicial em UTC.</param>
    /// <param name="toUtc">Data final em UTC.</param>
    /// <param name="take">Quantidade máxima de registros a retornar (padrão 20).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista dos produtos mais vendidos ordenados por quantidade.</returns>
    [HttpGet("top-products")]
    [Authorize(Policy = KnownPermissions.ReportView)]
    [ProducesResponseType(typeof(IReadOnlyList<TopProductReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TopProducts(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetTopProductsReportQuery(fromUtc, toUtc, take), cancellationToken);
        return Ok(rows);
    }

    /// <summary>
    /// Gera o relatório do fluxo de caixa (entradas e saídas) em um período específico.
    /// </summary>
    /// <param name="fromUtc">Data inicial em UTC.</param>
    /// <param name="toUtc">Data final em UTC.</param>
    /// <param name="take">Quantidade máxima de registros a retornar (padrão 100).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo as movimentações financeiras do fluxo de caixa.</returns>
    [HttpGet("cashflow")]
    [Authorize(Policy = KnownPermissions.CashflowView)]
    [ProducesResponseType(typeof(IReadOnlyList<CashFlowReportRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CashFlow(
        [FromQuery] DateTime fromUtc,
        [FromQuery] DateTime toUtc,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetCashFlowReportQuery(fromUtc, toUtc, take), cancellationToken);
        return Ok(rows);
    }

    /// <summary>
    /// Retorna a situação atual do estoque de todas as variações de produtos ativos.
    /// </summary>
    /// <param name="take">Quantidade máxima de registros a retornar (padrão 500).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo as informações de quantidade de estoque e valor unitário das variações.</returns>
    [HttpGet("stock")]
    [Authorize(Policy = KnownPermissions.ReportView)]
    [ProducesResponseType(typeof(IReadOnlyList<StockReportRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stock([FromQuery] int take = 500, CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetStockReportQuery(take), cancellationToken);
        return Ok(rows);
    }
}

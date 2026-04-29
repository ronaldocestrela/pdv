using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Reports;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _mediator;

    public ReportsController(ISender mediator)
    {
        _mediator = mediator;
    }

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

    [HttpGet("stock")]
    [Authorize(Policy = KnownPermissions.ReportView)]
    [ProducesResponseType(typeof(IReadOnlyList<StockReportRowDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stock([FromQuery] int take = 500, CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetStockReportQuery(take), cancellationToken);
        return Ok(rows);
    }
}

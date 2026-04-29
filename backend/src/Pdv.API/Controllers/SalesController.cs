using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Sales;
using Pdv.Application.Queries.Sales;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/sales")]
public sealed class SalesController : ControllerBase
{
    private readonly ISender _mediator;

    public SalesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = KnownPermissions.SaleCreate)]
    [ProducesResponseType(typeof(CreateSaleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Items.ConvertAll(i => new CreateSaleLineDto(i.ProductVariationId, i.Quantity));
        var result = await _mediator.Send(new CreateSaleCommand(lines, request.PaymentMethod), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Policy = KnownPermissions.SaleView)]
    [ProducesResponseType(typeof(IReadOnlyList<SaleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetSalesQuery(take), cancellationToken);
        return Ok(rows);
    }
}

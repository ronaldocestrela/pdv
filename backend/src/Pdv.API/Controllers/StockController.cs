using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Stock;
using Pdv.Application.Queries.Stock;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockController : ControllerBase
{
    private readonly ISender _mediator;

    public StockController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("adjust")]
    [Authorize(Policy = KnownPermissions.StockAdjust)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Adjust([FromBody] AddStockRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AddStockCommand(request.ProductVariationId, request.Quantity, request.Reason),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("movements")]
    [Authorize(Policy = KnownPermissions.StockView)]
    [ProducesResponseType(typeof(IReadOnlyList<StockMovementListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Movements(
        [FromQuery] int? variationId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetStockMovementsQuery(variationId, take), cancellationToken);
        return Ok(rows);
    }
}

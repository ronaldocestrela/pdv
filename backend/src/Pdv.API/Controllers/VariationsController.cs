using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Commands.Variations;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/variations")]
public sealed class VariationsController : ControllerBase
{
    private readonly ISender _mediator;

    public VariationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = KnownPermissions.VariationCreate)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateVariationRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateVariationCommand(request.ProductId, request.Name, request.Barcode, request.StockQuantity),
            cancellationToken);
        return Created($"/api/variations/{id}", new IdResponse(id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = KnownPermissions.VariationUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVariationRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateVariationCommand(id, request.Name, request.Barcode, request.StockQuantity),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = KnownPermissions.VariationDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteVariationCommand(id), cancellationToken);
        return NoContent();
    }

    public sealed record IdResponse(int Id);
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Products;
using Pdv.Application.Queries.Products;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _mediator;

    public ProductsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = KnownPermissions.ProductView)]
    [ProducesResponseType(typeof(IReadOnlyList<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = KnownPermissions.ProductView)]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = KnownPermissions.ProductCreate)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProductCommand(request.Name, request.IsActive), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse(id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = KnownPermissions.ProductUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateProductCommand(id, request.Name, request.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = KnownPermissions.ProductDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Wrapper for ProblemDetails-friendly created responses.</summary>
    public sealed record IdResponse(int Id);
}

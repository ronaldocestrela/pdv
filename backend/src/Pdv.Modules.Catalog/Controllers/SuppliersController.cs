using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Catalog.Controllers.Contracts;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;
using Pdv.Modules.Catalog.Application.Queries.Suppliers;
using Pdv.Shared.Kernel.Security;
using Pdv.Shared.Kernel.DTOs;

namespace Pdv.Modules.Catalog.Controllers;

/// <summary>
/// Controller responsible for managing supplier operations (CRUD).
/// </summary>
[ApiController]
[Route("api/suppliers")]
public sealed class SuppliersController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Retrieves a list of all suppliers registered for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of supplier summary DTOs.</returns>
    [HttpGet]
    [Authorize(Policy = KnownPermissions.SupplierView)]
    [ProducesResponseType(typeof(IReadOnlyList<SupplierSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSuppliersQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the details of a specific supplier by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The supplier details or 404 if not found.</returns>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = KnownPermissions.SupplierView)]
    [ProducesResponseType(typeof(SupplierSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Creates a new supplier for the current tenant.
    /// </summary>
    /// <param name="request">The payload with the supplier data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response containing the newly created supplier's ID.</returns>
    [HttpPost]
    [Authorize(Policy = KnownPermissions.SupplierCreate)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateSupplierCommand(
            request.Name,
            request.Document,
            request.Email,
            request.Phone,
            request.IsActive), cancellationToken);
            
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse(id));
    }

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to update.</param>
    /// <param name="request">The updated supplier payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = KnownPermissions.SupplierUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateSupplierCommand(
            id,
            request.Name,
            request.Document,
            request.Email,
            request.Phone,
            request.IsActive), cancellationToken);
            
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing supplier.
    /// </summary>
    /// <param name="id">The unique identifier of the supplier to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A 204 No Content response if successful.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = KnownPermissions.SupplierDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSupplierCommand(id), cancellationToken);
        return NoContent();
    }
}

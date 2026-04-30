using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Roles;
using Pdv.Application.Queries.Roles;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _mediator;

    public RolesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = PermissionsController.AdminRolesReadPolicy)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetRolesQuery(), cancellationToken);
        return Ok(rows);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = PermissionsController.AdminRolesReadPolicy)]
    [ProducesResponseType(typeof(RoleAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var row = await _mediator.Send(new GetRoleByIdQuery(id), cancellationToken);
        if (row is null)
            return NotFound();
        return Ok(row);
    }

    [HttpPost]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateRoleCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse(id));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateRoleCommand(id, request.Name), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:int}/permissions")]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPermissions(int id, [FromBody] SetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetRolePermissionsCommand(id, request.PermissionNames), cancellationToken);
        return NoContent();
    }
}

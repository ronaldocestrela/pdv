using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Users;
using Pdv.Application.Queries.Users;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = KnownPermissions.UserManage)]
    [ProducesResponseType(typeof(IReadOnlyList<UserAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(rows);
    }

    [HttpPost]
    [Authorize(Policy = KnownPermissions.UserManage)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateUserCommand(request.Email, request.Password, request.IsActive),
            cancellationToken);
        return Created($"/api/users/{id}", new IdResponse(id));
    }

    [HttpPut("{id:int}/roles")]
    [Authorize(Policy = KnownPermissions.UserManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetRoles(int id, [FromBody] SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetUserRolesCommand(id, request.RoleIds), cancellationToken);
        return NoContent();
    }
}

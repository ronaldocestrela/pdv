using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.Application.Queries.Roles;

namespace Pdv.API.Controllers;

[ApiController]
[Route("api/permissions")]
public sealed class PermissionsController : ControllerBase
{
    public const string AdminRolesReadPolicy = "admin.roles.read";

    private readonly ISender _mediator;

    public PermissionsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = AdminRolesReadPolicy)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCatalog(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetPermissionsCatalogQuery(), cancellationToken);
        return Ok(rows);
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Identity.Application.Queries.Roles;

namespace Pdv.Modules.Identity.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento e consulta do catálogo global de permissões do sistema.
/// </summary>
[ApiController]
[Route("api/permissions")]
public sealed class PermissionsController(ISender mediator) : ControllerBase
{
    public const string AdminRolesReadPolicy = "admin.roles.read";

    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Retorna a lista com todas as permissões cadastradas no sistema.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo os nomes das permissões em formato string.</returns>
    [HttpGet]
    [Authorize(Policy = AdminRolesReadPolicy)]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListCatalog(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetPermissionsCatalogQuery(), cancellationToken);
        return Ok(rows);
    }
}

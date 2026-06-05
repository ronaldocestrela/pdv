using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.Application.Queries.Roles;

namespace Pdv.API.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento e consulta do catálogo global de permissões do sistema.
/// </summary>
[ApiController]
[Route("api/permissions")]
public sealed class PermissionsController : ControllerBase
{
    public const string AdminRolesReadPolicy = "admin.roles.read";

    private readonly ISender _mediator;

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="PermissionsController"/>.
    /// </summary>
    /// <param name="mediator">Instância do remetente do MediatR para processamento de CQRS.</param>
    public PermissionsController(ISender mediator)
    {
        _mediator = mediator;
    }

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

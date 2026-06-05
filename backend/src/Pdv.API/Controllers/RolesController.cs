using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Roles;
using Pdv.Application.Queries.Roles;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

/// <summary>
/// Controller responsável por gerenciar os perfis de acesso (Roles) e suas permissões associadas.
/// </summary>
[ApiController]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _mediator;

    /// <summary>
    /// Inicializa uma nova instância da classe <see cref="RolesController"/>.
    /// </summary>
    /// <param name="mediator">Instância do remetente do MediatR para processamento de CQRS.</param>
    public RolesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna a lista de todos os perfis de acesso (Roles) cadastrados para o tenant atual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista de objetos contendo os perfis de acesso e suas respectivas permissões.</returns>
    [HttpGet]
    [Authorize(Policy = PermissionsController.AdminRolesReadPolicy)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetRolesQuery(), cancellationToken);
        return Ok(rows);
    }

    /// <summary>
    /// Retorna os detalhes de um perfil de acesso específico baseado no seu identificador.
    /// </summary>
    /// <param name="id">O ID do perfil de acesso.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O perfil de acesso solicitado ou 404 se não for encontrado.</returns>
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

    /// <summary>
    /// Cria um novo perfil de acesso (Role) para o tenant atual.
    /// </summary>
    /// <param name="request">Os dados contendo o nome do novo perfil.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O identificador do perfil recém-criado.</returns>
    [HttpPost]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateRoleCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse(id));
    }

    /// <summary>
    /// Atualiza o nome de um perfil de acesso existente.
    /// </summary>
    /// <param name="id">O ID do perfil de acesso.</param>
    /// <param name="request">O objeto contendo o novo nome para o perfil.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se atualizado com sucesso.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateRoleCommand(id, request.Name), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Remove um perfil de acesso (Role) do sistema, caso não seja um perfil protegido.
    /// </summary>
    /// <param name="id">O ID do perfil de acesso a ser removido.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se removido com sucesso.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Define e atualiza o conjunto de permissões atribuídas a um perfil de acesso (Role).
    /// </summary>
    /// <param name="id">O ID do perfil de acesso.</param>
    /// <param name="request">O objeto contendo a lista com os nomes das novas permissões.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se as permissões forem configuradas com sucesso.</returns>
    [HttpPut("{id:int}/permissions")]
    [Authorize(Policy = KnownPermissions.RoleManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetPermissions(int id, [FromBody] SetRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetRolePermissionsCommand(id, request.PermissionNames), cancellationToken);
        return NoContent();
    }
}

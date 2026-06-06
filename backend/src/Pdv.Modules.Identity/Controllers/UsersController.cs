using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Identity.Controllers.Contracts;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Users;
using Pdv.Modules.Identity.Application.Queries.Users;
using Pdv.Shared.Kernel.Security;
using Pdv.Shared.Kernel.DTOs;

namespace Pdv.Modules.Identity.Controllers;

/// <summary>
/// Controller responsável por expor as operações de administração de usuários (listar, criar e definir perfis).
/// </summary>
[ApiController]
[Route("api/users")]
public sealed class UsersController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Retorna a listagem de todos os usuários com suas respectivas roles para o tenant atual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo os dados administrativos dos usuários.</returns>
    [HttpGet]
    [Authorize(Policy = KnownPermissions.UserManage)]
    [ProducesResponseType(typeof(IReadOnlyList<UserAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(rows);
    }

    /// <summary>
    /// Cadastra um novo usuário no sistema associado ao tenant atual.
    /// </summary>
    /// <param name="request">O objeto contendo o e-mail, senha de acesso e estado de ativação del novo usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O identificador do usuário recém-criado.</returns>
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

    /// <summary>
    /// Define e atualiza as atribuições de perfis de acesso (Roles) de um usuário específico.
    /// </summary>
    /// <param name="id">O ID do usuário a ter as roles atualizadas.</param>
    /// <param name="request">O objeto contendo a lista com os IDs das novas roles.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se as atribuições forem efetuadas com sucesso.</returns>
    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = KnownPermissions.UserManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetUserRolesRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetUserRolesCommand(id, request.RoleIds), cancellationToken);
        return NoContent();
    }
}

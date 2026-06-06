using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Tenants;
using Pdv.Modules.Identity.Application.Queries.Tenants;
using Pdv.Modules.Identity.Controllers.Contracts;
using Pdv.Shared.Kernel.DTOs;
using Pdv.Shared.Kernel.Security;

namespace Pdv.Modules.Identity.Controllers;

/// <summary>
/// Controller responsável pelo cadastro e gestão de tenants.
/// Expõe endpoint público de auto-registro e endpoints protegidos para o Super Admin global.
/// </summary>
[ApiController]
[Route("api/tenants")]
public sealed class TenantsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Endpoint público de auto-registro de novo tenant.
    /// Cria a empresa, a role Super Admin local e o primeiro usuário administrador.
    /// </summary>
    /// <param name="request">Dados do novo tenant: nome da empresa, e-mail e senha do admin.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O ID do tenant recém-criado com status 201.</returns>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterTenantRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateTenantCommand(request.Name, request.AdminEmail, request.AdminPassword),
            cancellationToken);
        return Created($"/api/tenants/{id}", new IdResponse(id));
    }

    /// <summary>
    /// Cria um novo tenant via painel administrativo do Super Admin global.
    /// Requer permissão <c>tenant.manage</c>.
    /// </summary>
    /// <param name="request">Dados do novo tenant: nome da empresa, e-mail e senha do admin.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O ID do tenant recém-criado com status 201.</returns>
    [Authorize(Policy = KnownPermissions.TenantManage)]
    [HttpPost]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] RegisterTenantRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateTenantCommand(request.Name, request.AdminEmail, request.AdminPassword),
            cancellationToken);
        return Created($"/api/tenants/{id}", new IdResponse(id));
    }

    /// <summary>
    /// Lista todos os tenants cadastrados no sistema.
    /// Requer permissão <c>tenant.manage</c>.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de todos os tenants com ID, nome, status e data de criação.</returns>
    [Authorize(Policy = KnownPermissions.TenantManage)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TenantAdminDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var rows = await _mediator.Send(new GetTenantsQuery(), cancellationToken);
        return Ok(rows);
    }

    /// <summary>
    /// Ativa ou desativa um tenant pelo ID.
    /// Requer permissão <c>tenant.manage</c>.
    /// </summary>
    /// <param name="id">ID do tenant a ser modificado.</param>
    /// <param name="request">Objeto com o flag de ativação desejado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>204 No Content se a operação for bem-sucedida; 404 se o tenant não for encontrado.</returns>
    [Authorize(Policy = KnownPermissions.TenantManage)]
    [HttpPut("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActive(Guid id, [FromBody] SetTenantActiveRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetTenantActiveCommand(id, request.IsActive), cancellationToken);
        return NoContent();
    }
}

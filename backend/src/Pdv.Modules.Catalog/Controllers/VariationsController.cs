using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Catalog.Controllers.Contracts;
using Pdv.Modules.Catalog.Application.Commands.Variations;
using Pdv.Shared.Kernel.Security;
using Pdv.Shared.Kernel.DTOs;

namespace Pdv.Modules.Catalog.Controllers;

/// <summary>
/// Controller responsável por expor as operações de gerenciamento de variações de produtos (criação, edição e exclusão).
/// </summary>
[ApiController]
[Route("api/variations")]
public sealed class VariationsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Cria uma nova variação para um produto específico no tenant atual.
    /// </summary>
    /// <param name="request">O objeto contendo os detalhes da variação (ProductId, nome, preço unitário, estoque inicial, código de barras opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O identificador da variação recém-criada.</returns>
    [HttpPost]
    [Authorize(Policy = KnownPermissions.VariationCreate)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateVariationRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(
            new CreateVariationCommand(request.ProductId, request.Name, request.Barcode, request.StockQuantity, request.UnitPrice),
            cancellationToken);
        return Created($"/api/variations/{id}", new IdResponse(id));
    }

    /// <summary>
    /// Atualiza os dados de uma variação de produto existente.
    /// </summary>
    /// <param name="id">O ID da variação a ser atualizada.</param>
    /// <param name="request">O objeto contendo os novos dados da variação.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se atualizado com sucesso.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = KnownPermissions.VariationUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVariationRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateVariationCommand(id, request.Name, request.Barcode, request.StockQuantity, request.UnitPrice),
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Exclui uma variação de produto específica com base no seu identificador.
    /// </summary>
    /// <param name="id">O ID da variação de produto a ser removida.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se removido com sucesso.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = KnownPermissions.VariationDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteVariationCommand(id), cancellationToken);
        return NoContent();
    }
}

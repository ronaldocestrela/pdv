using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Commands.Variations;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

/// <summary>
/// Controller responsável por expor as operações de gerenciamento de variações de produtos (criação, edição e exclusão).
/// </summary>
/// <remarks>
/// Inicializa uma nova instância da classe <see cref="VariationsController"/>.
/// </remarks>
/// <param name="mediator">Instância do remetente do MediatR para processamento de CQRS.</param>
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
    [HttpPut("{id:int}")]
    [Authorize(Policy = KnownPermissions.VariationUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateVariationRequest request, CancellationToken cancellationToken)
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
    [HttpDelete("{id:int}")]
    [Authorize(Policy = KnownPermissions.VariationDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteVariationCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>Wrapper para respostas padronizadas que contêm apenas um identificador.</summary>
    public sealed record IdResponse(int Id);
}

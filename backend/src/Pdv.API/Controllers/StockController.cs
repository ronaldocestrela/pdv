using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pdv.API.Contracts;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Stock;
using Pdv.Application.Queries.Stock;
using Pdv.Application.Security;

namespace Pdv.API.Controllers;

/// <summary>
/// Controller responsável por expor as operações de estoque (ajuste e histórico de movimentações).
/// </summary>
/// <remarks>
/// Inicializa uma nova instância da classe <see cref="StockController"/>.
/// </remarks>
/// <param name="mediator">Instância do remetente do MediatR para processamento de CQRS.</param>
[ApiController]
[Route("api/stock")]
public sealed class StockController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Ajusta o estoque de uma variação de produto específica, registrando uma entrada (IN) ou saída (OUT) de produtos.
    /// </summary>
    /// <param name="request">O objeto contendo o ID da variação, a quantidade a ajustar e o motivo.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>204 No Content se o ajuste de estoque for efetuado com sucesso.</returns>
    [HttpPost("adjust")]
    [Authorize(Policy = KnownPermissions.StockAdjust)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Adjust([FromBody] AddStockRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AddStockCommand(request.ProductVariationId, request.Quantity, request.Reason),
            cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retorna o histórico de todas as movimentações de estoque registradas para o tenant atual.
    /// </summary>
    /// <param name="variationId">Filtro opcional para listar apenas movimentações de uma variação específica.</param>
    /// <param name="take">Quantidade máxima de registros a retornar (padrão 100).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo o histórico das movimentações de estoque encontradas.</returns>
    [HttpGet("movements")]
    [Authorize(Policy = KnownPermissions.StockView)]
    [ProducesResponseType(typeof(IReadOnlyList<StockMovementListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Movements(
        [FromQuery] int? variationId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetStockMovementsQuery(variationId, take), cancellationToken);
        return Ok(rows);
    }
}

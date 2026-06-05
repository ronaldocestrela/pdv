using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Sales.Controllers.Contracts;
using Pdv.Modules.Sales.Application.Abstractions;
using Pdv.Modules.Sales.Application.Commands.Sales;
using Pdv.Modules.Sales.Application.Queries.Sales;
using Pdv.Shared.Kernel.Security;

namespace Pdv.Modules.Sales.Controllers;

/// <summary>
/// Controller responsável por expor as operações de vendas do PDV (criação de vendas e histórico).
/// </summary>
[ApiController]
[Route("api/sales")]
public sealed class SalesController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Registra uma nova venda no PDV, realizando a baixa de estoque, gerando movimentações e lançando entrada no caixa.
    /// </summary>
    /// <param name="request">O objeto contendo os itens da venda (variações e quantidades) e a forma de pagamento.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O resultado contendo o identificador da venda criada e o valor total.</returns>
    [HttpPost]
    [Authorize(Policy = KnownPermissions.SaleCreate)]
    [ProducesResponseType(typeof(CreateSaleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var lines = request.Items.ConvertAll(i => new CreateSaleLineDto(i.ProductVariationId, i.Quantity));
        var result = await _mediator.Send(new CreateSaleCommand(lines, request.PaymentMethod), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna o histórico das últimas vendas cadastradas para o tenant atual.
    /// </summary>
    /// <param name="take">Quantidade máxima de registros a retornar (padrão 100).</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo informações resumidas das vendas realizadas.</returns>
    [HttpGet]
    [Authorize(Policy = KnownPermissions.SaleView)]
    [ProducesResponseType(typeof(IReadOnlyList<SaleListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int take = 100, CancellationToken cancellationToken = default)
    {
        var rows = await _mediator.Send(new GetSalesQuery(take), cancellationToken);
        return Ok(rows);
    }
}

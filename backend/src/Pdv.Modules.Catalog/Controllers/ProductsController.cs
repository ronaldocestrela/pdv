using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pdv.Modules.Catalog.Controllers.Contracts;
using Pdv.Modules.Catalog.Application.Abstractions;
using Pdv.Modules.Catalog.Application.Commands.Products;
using Pdv.Modules.Catalog.Application.Queries.Products;
using Pdv.Shared.Kernel.Security;
using Pdv.Shared.Kernel.DTOs;

namespace Pdv.Modules.Catalog.Controllers;

/// <summary>
/// Controller responsável por expor as operações de gerenciamento de produtos (CRUD e consulta do catálogo).
/// </summary>
[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    /// <summary>
    /// Retorna uma lista de resumos de todos os produtos cadastrados para o tenant atual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma lista contendo resumos simples dos produtos cadastrados.</returns>
    [HttpGet]
    [Authorize(Policy = KnownPermissions.ProductView)]
    [ProducesResponseType(typeof(IReadOnlyList<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retorna os detalhes de um produto específico com base no seu identificador exclusivo, incluindo suas variações.
    /// </summary>
    /// <param name="id">O ID do produto a ser pesquisado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Os detalhes do produto encontrado ou 404 se não for encontrado.</returns>
    [HttpGet("{id:int}")]
    [Authorize(Policy = KnownPermissions.ProductView)]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (result is null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Cadastra um novo produto para o tenant associado à requisição.
    /// </summary>
    /// <param name="request">O objeto contendo os dados do produto a ser criado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>O ID e localizador do recurso recém-criado.</returns>
    [HttpPost]
    [Authorize(Policy = KnownPermissions.ProductCreate)]
    [ProducesResponseType(typeof(IdResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateProductCommand(request.Name, request.IsActive), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new IdResponse(id));
    }

    /// <summary>
    /// Altera as propriedades de um produto existente com base no seu identificador.
    /// </summary>
    /// <param name="id">O ID do produto a ser alterado.</param>
    /// <param name="request">O objeto contendo os novos dados do produto.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma resposta 204 No Content se atualizado com sucesso.</returns>
    [HttpPut("{id:int}")]
    [Authorize(Policy = KnownPermissions.ProductUpdate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateProductCommand(id, request.Name, request.IsActive), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Exclui permanentemente um produto do sistema com base no seu identificador exclusivo.
    /// </summary>
    /// <param name="id">O ID do produto a ser deletado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    /// <returns>Uma resposta 204 No Content se a remoção for concluída com sucesso.</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = KnownPermissions.ProductDelete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return NoContent();
    }
}

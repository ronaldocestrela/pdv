using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Variations;

namespace Pdv.Application.Handlers.Variations;

public sealed class DeleteVariationCommandHandler : IRequestHandler<DeleteVariationCommand, Unit>
{
    private readonly IProductRepository _products;

    public DeleteVariationCommandHandler(IProductRepository products)
    {
        _products = products;
    }

    public async Task<Unit> Handle(DeleteVariationCommand request, CancellationToken cancellationToken)
    {
        var v = await _products.GetTrackedVariationByIdAsync(request.Id, cancellationToken);
        if (v is null)
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "Variação não encontrada.")]);

        _products.RemoveVariation(v);
        await _products.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

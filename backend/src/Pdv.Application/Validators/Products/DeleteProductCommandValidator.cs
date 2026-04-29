using FluentValidation;
using Pdv.Application.Commands.Products;

namespace Pdv.Application.Validators.Products;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

using FluentValidation;
using Pdv.Application.Commands.Products;

namespace Pdv.Application.Validators.Products;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);
    }
}

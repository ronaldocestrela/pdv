using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Products;

namespace Pdv.Modules.Catalog.Application.Validators.Products;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateProductCommandValidator"/> class.
    /// </summary>
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);
    }
}

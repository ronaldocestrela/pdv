using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Products;

namespace Pdv.Modules.Catalog.Application.Validators.Products;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateProductCommandValidator"/> class.
    /// </summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);
    }
}

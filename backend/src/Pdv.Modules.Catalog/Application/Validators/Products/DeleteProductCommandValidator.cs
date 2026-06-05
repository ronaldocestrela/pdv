using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Products;

namespace Pdv.Modules.Catalog.Application.Validators.Products;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteProductCommandValidator"/> class.
    /// </summary>
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

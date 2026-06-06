using FluentValidation;
using Pdv.Modules.Catalog.Application.Commands.Variations;

namespace Pdv.Modules.Catalog.Application.Validators.Variations;

public sealed class DeleteVariationCommandValidator : AbstractValidator<DeleteVariationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteVariationCommandValidator"/> class.
    /// </summary>
    public DeleteVariationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

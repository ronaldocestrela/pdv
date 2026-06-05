using FluentValidation;
using Pdv.Application.Commands.Variations;

namespace Pdv.Application.Validators.Variations;

public sealed class DeleteVariationCommandValidator : AbstractValidator<DeleteVariationCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteVariationCommandValidator"/> class.
    /// </summary>
    public DeleteVariationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

using FluentValidation;
using Pdv.Application.Commands.Variations;

namespace Pdv.Application.Validators.Variations;

public sealed class DeleteVariationCommandValidator : AbstractValidator<DeleteVariationCommand>
{
    public DeleteVariationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

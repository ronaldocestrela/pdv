using FluentValidation;
using Pdv.Application.Queries.Products;

namespace Pdv.Application.Validators.Products;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

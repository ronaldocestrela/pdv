using FluentValidation;
using Pdv.Application.Queries.Products;

namespace Pdv.Application.Validators.Products;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetProductByIdQueryValidator"/> class.
    /// </summary>
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}

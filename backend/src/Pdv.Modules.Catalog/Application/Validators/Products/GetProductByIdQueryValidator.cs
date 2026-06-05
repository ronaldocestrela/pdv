using FluentValidation;
using Pdv.Modules.Catalog.Application.Queries.Products;

namespace Pdv.Modules.Catalog.Application.Validators.Products;

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

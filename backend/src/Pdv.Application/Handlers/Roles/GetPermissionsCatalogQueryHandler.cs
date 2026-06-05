using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Roles;

namespace Pdv.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="GetPermissionsCatalogQueryHandler"/> class.
/// </summary>
public sealed class GetPermissionsCatalogQueryHandler(IRoleRepository roles) : IRequestHandler<GetPermissionsCatalogQuery, IReadOnlyList<string>>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="GetPermissionsCatalog"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<string>> Handle(GetPermissionsCatalogQuery request, CancellationToken cancellationToken) =>
        _roles.ListPermissionNamesAsync(cancellationToken);
}

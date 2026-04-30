using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Roles;

namespace Pdv.Application.Handlers.Roles;

public sealed class GetPermissionsCatalogQueryHandler : IRequestHandler<GetPermissionsCatalogQuery, IReadOnlyList<string>>
{
    private readonly IRoleRepository _roles;

    public GetPermissionsCatalogQueryHandler(IRoleRepository roles)
    {
        _roles = roles;
    }

    public Task<IReadOnlyList<string>> Handle(GetPermissionsCatalogQuery request, CancellationToken cancellationToken) =>
        _roles.ListPermissionNamesAsync(cancellationToken);
}

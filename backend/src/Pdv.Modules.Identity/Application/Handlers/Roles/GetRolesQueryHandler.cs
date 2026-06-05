using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Queries.Roles;

namespace Pdv.Modules.Identity.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="GetRolesQueryHandler"/> class.
/// </summary>
public sealed class GetRolesQueryHandler(IRoleRepository roles) : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleAdminDto>>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="GetRoles"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<RoleAdminDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        _roles.ListRolesAsync(cancellationToken);
}

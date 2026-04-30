using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Roles;

namespace Pdv.Application.Handlers.Roles;

public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleAdminDto>>
{
    private readonly IRoleRepository _roles;

    public GetRolesQueryHandler(IRoleRepository roles)
    {
        _roles = roles;
    }

    public Task<IReadOnlyList<RoleAdminDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken) =>
        _roles.ListRolesAsync(cancellationToken);
}

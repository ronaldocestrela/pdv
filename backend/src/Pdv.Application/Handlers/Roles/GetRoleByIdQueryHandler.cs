using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Roles;

namespace Pdv.Application.Handlers.Roles;

public sealed class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleAdminDto?>
{
    private readonly IRoleRepository _roles;

    public GetRoleByIdQueryHandler(IRoleRepository roles)
    {
        _roles = roles;
    }

    public Task<RoleAdminDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken) =>
        _roles.GetRoleByIdAsync(request.Id, cancellationToken);
}

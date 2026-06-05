using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Queries.Roles;

namespace Pdv.Modules.Identity.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="GetRoleByIdQueryHandler"/> class.
/// </summary>
public sealed class GetRoleByIdQueryHandler(IRoleRepository roles) : IRequestHandler<GetRoleByIdQuery, RoleAdminDto?>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="GetRoleById"/> to retrieve the requested data.
    /// </summary>
    public Task<RoleAdminDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken) =>
        _roles.GetRoleByIdAsync(request.Id, cancellationToken);
}

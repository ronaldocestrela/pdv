using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Roles;
using Pdv.Application.Security;

namespace Pdv.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="SetRolePermissionsCommandHandler"/> class.
/// </summary>
public sealed class SetRolePermissionsCommandHandler(IRoleRepository roles) : IRequestHandler<SetRolePermissionsCommand, Unit>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="SetRolePermissions"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Unit> Handle(SetRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetTrackedByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            throw new ValidationException([new ValidationFailure(nameof(request.RoleId), "Role não encontrada.")]);

        if (string.Equals(role.Name, KnownRoles.SuperAdmin, StringComparison.Ordinal))
            throw new ValidationException([new ValidationFailure(nameof(request.RoleId), "Permissões da role Super Admin são geridas automaticamente.")]);

        await _roles.SetRolePermissionsByNamesAsync(request.RoleId, request.PermissionNames, cancellationToken);
        return Unit.Value;
    }
}

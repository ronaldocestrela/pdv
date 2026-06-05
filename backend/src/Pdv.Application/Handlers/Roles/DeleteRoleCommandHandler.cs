using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Roles;
using Pdv.Application.Security;

namespace Pdv.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="DeleteRoleCommandHandler"/> class.
/// </summary>
public sealed class DeleteRoleCommandHandler(IRoleRepository roles) : IRequestHandler<DeleteRoleCommand, Unit>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="DeleteRole"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetTrackedByIdAsync(request.Id, cancellationToken);
        if (role is null)
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "Role não encontrada.")]);

        if (string.Equals(role.Name, KnownRoles.SuperAdmin, StringComparison.Ordinal))
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "A role Super Admin não pode ser excluída.")]);

        _roles.Remove(role);
        await _roles.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

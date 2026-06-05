using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Roles;
using Pdv.Shared.Kernel.Security;

namespace Pdv.Modules.Identity.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="UpdateRoleCommandHandler"/> class.
/// </summary>
public sealed class UpdateRoleCommandHandler(IRoleRepository roles) : IRequestHandler<UpdateRoleCommand, Unit>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="UpdateRole"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Unit> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roles.GetTrackedByIdAsync(request.Id, cancellationToken);
        if (role is null)
            throw new ValidationException([new ValidationFailure(nameof(request.Id), "Role não encontrada.")]);

        if (string.Equals(role.Name, KnownRoles.SuperAdmin, StringComparison.Ordinal))
        {
            if (!string.Equals(request.Name.Trim(), KnownRoles.SuperAdmin, StringComparison.Ordinal))
                throw new ValidationException([new ValidationFailure(nameof(request.Name), "O nome da role Super Admin não pode ser alterado.")]);
            return Unit.Value;
        }

        var newName = request.Name.Trim();
        if (await _roles.NameExistsAsync(newName, request.Id, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(request.Name), "Já existe uma role com este nome.")]);

        role.Name = newName;
        await _roles.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

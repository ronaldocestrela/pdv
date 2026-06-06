using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Roles;
using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Modules.Identity.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="CreateRoleCommandHandler"/> class.
/// </summary>
public sealed class CreateRoleCommandHandler(IRoleRepository roles) : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="CreateRole"/> to perform the corresponding business action.
    /// </summary>
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();
        if (await _roles.NameExistsAsync(name, null, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(CreateRoleCommand.Name), "Já existe uma role com este nome.")]);

        var role = new Role { Name = name };
        _roles.Add(role);
        await _roles.SaveChangesAsync(cancellationToken);
        return role.Id;
    }
}

using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Roles;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Roles;

/// <summary>
/// Initializes a new instance of the <see cref="CreateRoleCommandHandler"/> class.
/// </summary>
public sealed class CreateRoleCommandHandler(IRoleRepository roles) : IRequestHandler<CreateRoleCommand, int>
{
    private readonly IRoleRepository _roles = roles;

    /// <summary>
    /// Executes the <see cref="CreateRole"/> to perform the corresponding business action.
    /// </summary>
    public async Task<int> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
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

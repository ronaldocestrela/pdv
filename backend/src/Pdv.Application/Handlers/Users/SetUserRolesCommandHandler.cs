using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Users;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Users;

public sealed class SetUserRolesCommandHandler : IRequestHandler<SetUserRolesCommand, Unit>
{
    private readonly IUserAdminRepository _users;

    public SetUserRolesCommandHandler(IUserAdminRepository users)
    {
        _users = users;
    }

    public async Task<Unit> Handle(SetUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetTrackedWithRolesAsync(request.UserId, cancellationToken);
        if (user is null)
            throw new ValidationException([new ValidationFailure(nameof(request.UserId), "Usuário não encontrado.")]);

        if (!await _users.AllRoleIdsExistAsync(request.RoleIds, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(request.RoleIds), "Uma ou mais roles são inválidas.")]);

        var distinct = request.RoleIds.Distinct().ToList();
        var existing = user.UserRoles.ToList();
        foreach (var ur in existing)
            user.UserRoles.Remove(ur);

        foreach (var rid in distinct)
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = rid });

        await _users.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

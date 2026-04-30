using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Users;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Users;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserAdminRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUserAdminRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<int> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        if (await _users.EmailExistsAsync(email, cancellationToken))
            throw new ValidationException([new ValidationFailure(nameof(CreateUserCommand.Email), "E-mail já cadastrado.")]);

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = request.IsActive,
        };
        _users.Add(user);
        await _users.SaveChangesAsync(cancellationToken);
        return user.Id;
    }
}

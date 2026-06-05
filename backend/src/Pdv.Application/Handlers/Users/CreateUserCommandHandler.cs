using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Commands.Users;
using Pdv.Domain.Entities;

namespace Pdv.Application.Handlers.Users;

/// <summary>
/// Initializes a new instance of the <see cref="CreateUserCommandHandler"/> class.
/// </summary>
public sealed class CreateUserCommandHandler(IUserAdminRepository users, IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, int>
{
    private readonly IUserAdminRepository _users = users;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;

    /// <summary>
    /// Executes the <see cref="CreateUser"/> to perform the corresponding business action.
    /// </summary>
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

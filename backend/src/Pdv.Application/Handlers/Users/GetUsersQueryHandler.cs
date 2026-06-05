using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Users;

namespace Pdv.Application.Handlers.Users;

/// <summary>
/// Initializes a new instance of the <see cref="GetUsersQueryHandler"/> class.
/// </summary>
public sealed class GetUsersQueryHandler(IUserAdminRepository users) : IRequestHandler<GetUsersQuery, IReadOnlyList<UserAdminDto>>
{
    private readonly IUserAdminRepository _users = users;

    /// <summary>
    /// Executes the <see cref="GetUsers"/> to retrieve the requested data.
    /// </summary>
    public Task<IReadOnlyList<UserAdminDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _users.ListUsersWithRolesAsync(cancellationToken);
}

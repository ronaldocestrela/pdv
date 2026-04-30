using MediatR;
using Pdv.Application.Abstractions;
using Pdv.Application.Queries.Users;

namespace Pdv.Application.Handlers.Users;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserAdminDto>>
{
    private readonly IUserAdminRepository _users;

    public GetUsersQueryHandler(IUserAdminRepository users)
    {
        _users = users;
    }

    public Task<IReadOnlyList<UserAdminDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken) =>
        _users.ListUsersWithRolesAsync(cancellationToken);
}

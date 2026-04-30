using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Users;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserAdminDto>>;

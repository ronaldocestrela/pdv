using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;

namespace Pdv.Modules.Identity.Application.Queries.Users;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserAdminDto>>;

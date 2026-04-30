using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Roles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleAdminDto>>;

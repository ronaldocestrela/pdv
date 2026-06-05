using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;

namespace Pdv.Modules.Identity.Application.Queries.Roles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleAdminDto>>;

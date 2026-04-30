using MediatR;
using Pdv.Application.Abstractions;

namespace Pdv.Application.Queries.Roles;

public sealed record GetRoleByIdQuery(int Id) : IRequest<RoleAdminDto?>;

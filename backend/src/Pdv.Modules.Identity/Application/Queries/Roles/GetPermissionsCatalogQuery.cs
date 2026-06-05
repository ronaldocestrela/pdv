using MediatR;

namespace Pdv.Modules.Identity.Application.Queries.Roles;

public sealed record GetPermissionsCatalogQuery : IRequest<IReadOnlyList<string>>;

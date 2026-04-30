using MediatR;

namespace Pdv.Application.Queries.Roles;

public sealed record GetPermissionsCatalogQuery : IRequest<IReadOnlyList<string>>;

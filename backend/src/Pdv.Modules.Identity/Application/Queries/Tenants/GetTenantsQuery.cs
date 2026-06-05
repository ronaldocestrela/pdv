using MediatR;
using Pdv.Modules.Identity.Application.Abstractions;

namespace Pdv.Modules.Identity.Application.Queries.Tenants;

/// <summary>
/// Query para listar todos os tenants cadastrados no sistema (uso exclusivo do Super Admin global).
/// </summary>
public sealed record GetTenantsQuery : IRequest<IReadOnlyList<TenantAdminDto>>;

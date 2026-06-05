using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Pdv.Application.Abstractions;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;

namespace Pdv.Infrastructure.Repositories;

/// <summary>
/// Initializes a new instance of the <see cref="RoleRepository"/> class.
/// </summary>
public sealed class RoleRepository(AppDbContext db) : IRoleRepository
{
    private readonly AppDbContext _db = db;

    /// <summary>
    /// Retrieves a list of DTO summaries.
    /// </summary>
    public async Task<IReadOnlyList<RoleAdminDto>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return roles
            .Select(r => new RoleAdminDto(
                r.Id,
                r.Name,
                r.RolePermissions.Select(rp => rp.Permission.Name).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList()))
            .ToList();
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public async Task<RoleAdminDto?> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var r = await _db.Roles
            .AsNoTracking()
            .Include(x => x.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (r is null)
            return null;

        return new RoleAdminDto(
            r.Id,
            r.Name,
            r.RolePermissions.Select(rp => rp.Permission.Name).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList());
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<Role?> GetTrackedWithPermissionsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _db.Roles
            .Include(x => x.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves tracking details by ID.
    /// </summary>
    public Task<Role?> GetTrackedByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    /// <summary>
    /// Executes the SetRolePermissionsByNamesAsync operation.
    /// </summary>
    public async Task SetRolePermissionsByNamesAsync(int roleId, IReadOnlyList<string> permissionNames, CancellationToken cancellationToken = default)
    {
        var distinctNames = permissionNames.Distinct(StringComparer.Ordinal).ToList();
        var validNames = await _db.Permissions.AsNoTracking()
            .Where(p => distinctNames.Contains(p.Name))
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);
        if (validNames.Count != distinctNames.Count)
        {
            var validSet = validNames.ToHashSet(StringComparer.Ordinal);
            var invalid = distinctNames.Where(n => !validSet.Contains(n)).ToList();
            throw new ValidationException(invalid.Select(n =>
                new ValidationFailure("permissionNames", $"Permissão desconhecida: {n}")));
        }

        var permIds = await _db.Permissions.AsNoTracking()
            .Where(p => distinctNames.Contains(p.Name))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        var toRemove = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        _db.RolePermissions.RemoveRange(toRemove);
        foreach (var pid in permIds)
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });

        await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the NameExistsAsync operation.
    /// </summary>
    public Task<bool> NameExistsAsync(string name, int? excludeRoleId, CancellationToken cancellationToken = default)
    {
        var q = _db.Roles.Where(r => r.Name == name);
        if (excludeRoleId is { } ex)
            q = q.Where(r => r.Id != ex);
        return q.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the database context.
    /// </summary>
    public void Add(Role role) => _db.Roles.Add(role);

    /// <summary>
    /// Removes the specified entity from the database context.
    /// </summary>
    public void Remove(Role role) => _db.Roles.Remove(role);

    /// <summary>
    /// Retrieves a list of DTO summaries.
    /// </summary>
    public async Task<IReadOnlyList<string>> ListPermissionNamesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Persists all tracked changes in this database context.
    /// </summary>
    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}

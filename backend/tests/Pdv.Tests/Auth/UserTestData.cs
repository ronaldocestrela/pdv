using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Tests.Auth;

internal static class UserTestData
{
    public static User CreateActiveUser(
        string email = "u@test.local",
        string passwordHash = "HASH_VALUE",
        string permissionName = "product.view")
    {
        var permission = new Permission { Id = Guid.NewGuid(), Name = permissionName };
        var role = new Role { Id = Guid.NewGuid(), Name = "TestRole" };

        role.RolePermissions.Add(new RolePermission
        {
            Role = role,
            Permission = permission,
        });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
        };

        user.UserRoles.Add(new UserRole
        {
            User = user,
            Role = role,
        });

        return user;
    }
}

using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Tests.Auth;

internal static class UserTestData
{
    public static User CreateActiveUser(
        string email = "u@test.local",
        string passwordHash = "HASH_VALUE",
        string permissionName = "product.view")
    {
        var permission = new Permission { Id = 1, Name = permissionName };
        var role = new Role { Id = 1, Name = "TestRole" };

        role.RolePermissions.Add(new RolePermission
        {
            Role = role,
            Permission = permission,
        });

        var user = new User
        {
            Id = 1,
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

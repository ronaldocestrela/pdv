using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Identity.Application.Commands.Roles;
using Pdv.Modules.Identity.Application.Commands.Users;
using Pdv.Modules.Identity.Application.Handlers.Roles;
using Pdv.Modules.Identity.Application.Handlers.Users;
using Pdv.Shared.Kernel.Security;
using Pdv.Modules.Identity.Domain.Entities;
using Pdv.Modules.Identity.Infrastructure.Persistence;
using Pdv.Modules.Identity.Infrastructure.Persistence.Repositories;
using Pdv.Modules.Identity.Infrastructure.Services;

namespace Pdv.Tests.Admin;

public sealed class AdminRolesAndUsersTests
{
    [Fact]
    public async Task CreateRole_Then_SetPermissions_Persists()
    {
        await using var ctx = NewDb();
        SeedPermissions(ctx);
        var roles = new RoleRepository(ctx);

        var create = new CreateRoleCommandHandler(roles);
        var id = await create.Handle(new CreateRoleCommand("Caixa"), CancellationToken.None);
        id.Should().BeGreaterThan(0);

        var setPerms = new SetRolePermissionsCommandHandler(roles);
        await setPerms.Handle(new SetRolePermissionsCommand(id, [KnownPermissions.ProductView]), CancellationToken.None);

        var row = await roles.GetRoleByIdAsync(id, CancellationToken.None);
        row.Should().NotBeNull();
        row!.Permissions.Should().Contain(KnownPermissions.ProductView);
    }

    [Fact]
    public async Task SetRolePermissions_Rejects_SuperAdmin()
    {
        await using var ctx = NewDb();
        SeedPermissions(ctx);
        ctx.Roles.Add(new Role { Name = KnownRoles.SuperAdmin });
        await ctx.SaveChangesAsync();
        var superId = ctx.Roles.First(r => r.Name == KnownRoles.SuperAdmin).Id;

        var roles = new RoleRepository(ctx);
        var setPerms = new SetRolePermissionsCommandHandler(roles);
        var act = async () => await setPerms.Handle(new SetRolePermissionsCommand(superId, [KnownPermissions.ProductView]), CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task SetUserRoles_Replaces_Assignments()
    {
        await using var ctx = NewDb();
        SeedPermissions(ctx);
        ctx.Users.Add(new User { Email = "u@test.com", PasswordHash = "x", IsActive = true });
        ctx.Roles.Add(new Role { Name = "A" });
        ctx.Roles.Add(new Role { Name = "B" });
        await ctx.SaveChangesAsync();
        var userId = ctx.Users.First().Id;
        var roleA = ctx.Roles.First(r => r.Name == "A").Id;
        var roleB = ctx.Roles.First(r => r.Name == "B").Id;

        var admin = new UserAdminRepository(ctx);
        var handler = new SetUserRolesCommandHandler(admin);
        await handler.Handle(new SetUserRolesCommand(userId, [roleA]), CancellationToken.None);
        (await ctx.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync())
            .Should().BeEquivalentTo([roleA]);

        await handler.Handle(new SetUserRolesCommand(userId, [roleB]), CancellationToken.None);
        (await ctx.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync())
            .Should().BeEquivalentTo([roleB]);
    }

    [Fact]
    public async Task CreateUser_Persists_And_Can_Login_Hash()
    {
        await using var ctx = NewDb();
        var admin = new UserAdminRepository(ctx);
        var hasher = new BcryptPasswordHasher();
        var handler = new CreateUserCommandHandler(admin, hasher);

        var id = await handler.Handle(new CreateUserCommand("novo@x.com", "secret12", true), CancellationToken.None);
        id.Should().BeGreaterThan(0);

        var user = await ctx.Users.AsNoTracking().FirstAsync(u => u.Id == id);
        user.Email.Should().Be("novo@x.com");
        user.IsActive.Should().BeTrue();
        hasher.Verify("secret12", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_Rejects_Duplicate_Email()
    {
        await using var ctx = NewDb();
        ctx.Users.Add(new User { Email = "dup@test.com", PasswordHash = "x", IsActive = true });
        await ctx.SaveChangesAsync();

        var admin = new UserAdminRepository(ctx);
        var handler = new CreateUserCommandHandler(admin, new BcryptPasswordHasher());
        var act = async () => await handler.Handle(new CreateUserCommand("dup@test.com", "secret12"), CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    private static void SeedPermissions(IdentityDbContext ctx)
    {
        foreach (var name in KnownPermissions.All)
            ctx.Permissions.Add(new Permission { Name = name });
        ctx.SaveChanges();
    }

    private static IdentityDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new IdentityDbContext(options, new Pdv.Shared.Kernel.Services.SystemTenantContext());
    }
}

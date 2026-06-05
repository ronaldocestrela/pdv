using FluentAssertions;
using Moq;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Auth;
using Pdv.Modules.Identity.Application.Handlers.Auth;
using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Tests.Auth;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenResponse_and_SavesRefresh()
    {
        var user = UserTestData.CreateActiveUser(email: "a@test", passwordHash: "$2a$BCRYPT");
        var mocks = BuildMocks(user, passwordOk: true);
        mocks.Jwt.Setup(j => j.CreateAccessToken(It.IsAny<User>())).Returns(("access-token", DateTime.UtcNow.AddMinutes(30)));
        mocks.Jwt.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        mocks.Jwt.Setup(j => j.GetRefreshTokenExpiresAtUtc()).Returns(DateTime.UtcNow.AddDays(7));

        var handler = new LoginCommandHandler(mocks.Users.Object, mocks.Pwd.Object, mocks.Jwt.Object);

        var result = await handler.Handle(new LoginCommand("a@test", "secret"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.Email.Should().Be("a@test");
        result.UserId.Should().Be(1);
        result.Permissions.Should().Contain("product.view");

        mocks.Users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        user.RefreshToken.Should().Be("refresh-token");
        user.RefreshTokenExpiresAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsNull()
    {
        var user = UserTestData.CreateActiveUser();
        var mocks = BuildMocks(user, passwordOk: false);

        var handler = new LoginCommandHandler(mocks.Users.Object, mocks.Pwd.Object, mocks.Jwt.Object);

        var result = await handler.Handle(new LoginCommand("a@test.local", "bad"), CancellationToken.None);

        result.Should().BeNull();
        mocks.Users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserInactive_ReturnsNull()
    {
        var user = UserTestData.CreateActiveUser();
        user.IsActive = false;
        var mocks = BuildMocks(user, passwordOk: true);

        var handler = new LoginCommandHandler(mocks.Users.Object, mocks.Pwd.Object, mocks.Jwt.Object);

        var result = await handler.Handle(new LoginCommand("a@test.local", "secret"), CancellationToken.None);

        result.Should().BeNull();
        mocks.Users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (
        Mock<IUserRepository> Users,
        Mock<IPasswordHasher> Pwd,
        Mock<IJwtService> Jwt) BuildMocks(User user, bool passwordOk)
    {
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetWithPermissionsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        users.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var pwd = new Mock<IPasswordHasher>();
        pwd.Setup(p => p.Verify(It.IsAny<string>(), user.PasswordHash)).Returns(passwordOk);

        return (users, pwd, new Mock<IJwtService>());
    }
}

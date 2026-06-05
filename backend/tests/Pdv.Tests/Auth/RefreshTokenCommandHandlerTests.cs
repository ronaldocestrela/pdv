using FluentAssertions;
using Moq;
using Pdv.Modules.Identity.Application.Abstractions;
using Pdv.Modules.Identity.Application.Commands.Auth;
using Pdv.Modules.Identity.Application.Handlers.Auth;
using Pdv.Modules.Identity.Domain.Entities;

namespace Pdv.Tests.Auth;

public sealed class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRefresh_ReturnsTokens()
    {
        var user = UserTestData.CreateActiveUser();
        user.RefreshToken = "stored-refresh";
        user.RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(1);

        var mocks = BuildMocks(user);
        mocks.Jwt.Setup(j => j.CreateAccessToken(It.IsAny<User>())).Returns(("new-access", DateTime.UtcNow.AddMinutes(30)));
        mocks.Jwt.Setup(j => j.GenerateRefreshToken()).Returns("new-refresh");
        mocks.Jwt.Setup(j => j.GetRefreshTokenExpiresAtUtc()).Returns(DateTime.UtcNow.AddDays(7));

        var handler = new RefreshTokenCommandHandler(mocks.Users.Object, mocks.Jwt.Object);

        var result = await handler.Handle(new RefreshTokenCommand("stored-refresh"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().Be("new-refresh");
        mocks.Users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredRefresh_ReturnsNull()
    {
        var user = UserTestData.CreateActiveUser();
        user.RefreshToken = "old";
        user.RefreshTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(-10);

        var mocks = BuildMocks(user);
        var handler = new RefreshTokenCommandHandler(mocks.Users.Object, mocks.Jwt.Object);

        var result = await handler.Handle(new RefreshTokenCommand("old"), CancellationToken.None);

        result.Should().BeNull();
        mocks.Users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownToken_ReturnsNull()
    {
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetWithPermissionsByRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var jwt = new Mock<IJwtService>();

        var handler = new RefreshTokenCommandHandler(users.Object, jwt.Object);

        var result = await handler.Handle(new RefreshTokenCommand("unknown"), CancellationToken.None);

        result.Should().BeNull();
        users.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (Mock<IUserRepository> Users, Mock<IJwtService> Jwt) BuildMocks(User user)
    {
        var users = new Mock<IUserRepository>();
        users.Setup(u => u.GetWithPermissionsByRefreshTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        users.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return (users, new Mock<IJwtService>());
    }
}

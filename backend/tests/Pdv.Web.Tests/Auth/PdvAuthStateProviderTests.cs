using System.Security.Claims;
using Blazored.LocalStorage;
using FluentAssertions;
using Moq;
using Pdv.Web.Auth;
using Pdv.Web.Models;
using Xunit;

namespace Pdv.Web.Tests.Auth;

public class PdvAuthStateProviderTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage = new(MockBehavior.Strict);
    private readonly PdvAuthStateProvider _provider;

    public PdvAuthStateProviderTests()
    {
        _provider = new PdvAuthStateProvider(_mockLocalStorage.Object);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenNoSession_ShouldReturnUnauthenticated()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItemAsync<AuthSession>("pdv-auth", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthSession?)null);

        // Act
        var state = await _provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity.Should().NotBeNull();
        state.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_WhenValidSession_ShouldReturnAuthenticatedWithClaims()
    {
        // Arrange
        var session = new AuthSession
        {
            AccessToken = "valid-token",
            Email = "test@local",
            UserId = "user-123",
            TenantId = "tenant-456",
            Permissions = ["product.view", "sale.create"]
        };

        _mockLocalStorage.Setup(x => x.GetItemAsync<AuthSession>("pdv-auth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var state = await _provider.GetAuthenticationStateAsync();

        // Assert
        state.User.Identity.Should().NotBeNull();
        state.User.Identity!.IsAuthenticated.Should().BeTrue();
        state.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be("test@local");
        state.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("user-123");
        state.User.FindFirst("tenant_id")?.Value.Should().Be("tenant-456");
        state.User.FindAll("permission").Select(c => c.Value).Should().BeEquivalentTo("product.view", "sale.create");
    }

    [Fact]
    public async Task SetSessionAsync_ShouldSaveToLocalStorageAndNotify()
    {
        // Arrange
        var response = new AuthTokenResponse
        {
            AccessToken = "new-token",
            RefreshToken = "new-refresh",
            UserId = "user-123",
            Email = "test@local",
            TenantId = "tenant-456",
            Permissions = ["product.view"],
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1).ToString("o")
        };

        _mockLocalStorage.Setup(x => x.SetItemAsync("pdv-auth", It.IsAny<AuthSession>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        bool eventTriggered = false;
        _provider.AuthenticationStateChanged += (task) =>
        {
            eventTriggered = true;
        };

        // Act
        await _provider.SetSessionAsync(response);

        // Assert
        _mockLocalStorage.Verify(x => x.SetItemAsync("pdv-auth", It.Is<AuthSession>(s => 
            s.AccessToken == "new-token" && 
            s.RefreshToken == "new-refresh" && 
            s.UserId == "user-123" && 
            s.Email == "test@local" && 
            s.TenantId == "tenant-456"
        ), It.IsAny<CancellationToken>()), Times.Once);

        eventTriggered.Should().BeTrue();
    }

    [Fact]
    public async Task LogoutAsync_ShouldClearLocalStorageAndNotify()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.RemoveItemAsync("pdv-auth", It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        bool eventTriggered = false;
        _provider.AuthenticationStateChanged += (task) =>
        {
            eventTriggered = true;
        };

        // Act
        await _provider.LogoutAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("pdv-auth", It.IsAny<CancellationToken>()), Times.Once);
        eventTriggered.Should().BeTrue();

        var state = await _provider.GetAuthenticationStateAsync();
        state.User.Identity!.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public async Task Can_WhenPermissionExists_ShouldReturnTrue()
    {
        // Arrange
        var session = new AuthSession
        {
            AccessToken = "valid-token",
            Permissions = ["product.view"]
        };

        _mockLocalStorage.Setup(x => x.GetItemAsync<AuthSession>("pdv-auth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Populate internal state
        await _provider.GetAuthenticationStateAsync();

        // Act & Assert
        _provider.Can("product.view").Should().BeTrue();
        _provider.Can("product.create").Should().BeFalse();
    }
}

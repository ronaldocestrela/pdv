using System.Net;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Pdv.Web.Models;
using Pdv.Web.Pages;
using Xunit;

namespace Pdv.Web.Tests.Pages;

public class LoginTests : BlazorTestBase
{
    [Fact]
    public void Login_InitialRender_ShouldShowLoginForm()
    {
        // Act
        var cut = RenderComponent<Login>();

        // Assert
        cut.Find("h1.login-title").TextContent.Should().Be("PDV + Estoque");
        cut.Find("input[type='email']").Should().NotBeNull();
        cut.Find("input[type='password']").Should().NotBeNull();
        cut.Find("button[type='submit']").TextContent.Trim().Should().Be("Entrar");
    }

    [Fact]
    public void Login_TogglePasswordVisibility_ShouldChangeInputType()
    {
        // Arrange
        var cut = RenderComponent<Login>();
        var passwordInput = cut.Find("#password");
        passwordInput.GetAttribute("type").Should().Be("password");

        // Act - Click toggle button
        var toggleBtn = cut.Find(".login-toggle");
        toggleBtn.Click();

        // Assert - type changes to text
        passwordInput.GetAttribute("type").Should().Be("text");

        // Act - Click again
        toggleBtn.Click();

        // Assert - type changes back to password
        passwordInput.GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public async Task Login_OnSuccessfulSubmit_ShouldSetSessionAndRedirect()
    {
        // Arrange
        var tokenResponse = new AuthTokenResponse
        {
            AccessToken = "token-123",
            RefreshToken = "refresh-123",
            UserId = "user-123",
            Email = "admin@local",
            TenantId = "tenant-1",
            Permissions = ["product.view"],
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60).ToString("o")
        };

        // Setup mock HTTP response for login endpoint
        HttpHandler.Setup("POST", "/api/auth/login", HttpStatusCode.OK, tokenResponse);

        // Setup LocalStorage set session mock
        MockLocalStorage.Setup(x => x.SetItemAsync("pdv-auth", It.IsAny<AuthSession>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var cut = RenderComponent<Login>();

        // Fill form fields
        cut.Find("input[type='email']").Change("admin@local");
        cut.Find("#password").Input("password123");

        // Act - submit form
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert - redirection occurs
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().Be("http://localhost/");

        // Assert - provider holds session
        var state = await AuthProvider.GetAuthenticationStateAsync();
        state.User.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public async Task Login_OnInvalidCredentials_ShouldDisplayErrorMessage()
    {
        // Arrange
        // Mock 401 Unauthorized response from backend
        HttpHandler.Setup("POST", "/api/auth/login", HttpStatusCode.Unauthorized);

        var cut = RenderComponent<Login>();

        // Fill form
        cut.Find("input[type='email']").Change("wrong@email.com");
        cut.Find("#password").Input("wrongpass");

        // Act - submit
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert - error element rendered with correct text
        var errorEl = cut.Find("p.login-error");
        errorEl.TextContent.Should().Be("E-mail ou senha inválidos.");

        // Check auth status
        var state = await AuthProvider.GetAuthenticationStateAsync();
        state.User.Identity!.IsAuthenticated.Should().BeFalse();
    }
}

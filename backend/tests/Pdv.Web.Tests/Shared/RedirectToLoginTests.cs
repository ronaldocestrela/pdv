using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Pdv.Web.Shared;
using Xunit;

namespace Pdv.Web.Tests.Shared;

public class RedirectToLoginTests : BlazorTestBase
{
    [Fact]
    public void RedirectToLogin_OnInitialized_ShouldRedirectToLoginWithReturnUrl()
    {
        // Arrange
        var nav = Services.GetRequiredService<NavigationManager>();
        var currentUri = nav.Uri;

        // Act
        RenderComponent<RedirectToLogin>();

        // Assert
        var expectedRedirect = $"http://localhost/login?returnUrl={Uri.EscapeDataString(currentUri)}";
        nav.Uri.Should().Be(expectedRedirect);
    }
}

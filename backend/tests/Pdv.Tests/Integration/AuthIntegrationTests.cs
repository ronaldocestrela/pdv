using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Pdv.Tests.Integration;

public sealed class AuthIntegrationTests
{
    [Fact]
    public async Task Login_returns_unauthorized_when_password_wrong()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email = PdvWebApplicationFactory.TestAdminEmail, password = "WrongPassword!" },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_returns_tokens_when_credentials_valid()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email = PdvWebApplicationFactory.TestAdminEmail, password = PdvWebApplicationFactory.TestAdminPassword },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(WebJson.Options);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_returns_bad_request_when_email_invalid()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email = "not-an-email", password = "x" },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Refresh_returns_unauthorized_for_invalid_token()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/auth/refresh",
            new { refreshToken = "invalid-token" },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_returns_new_tokens_after_successful_login()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email = PdvWebApplicationFactory.TestAdminEmail, password = PdvWebApplicationFactory.TestAdminPassword },
            WebJson.Options);
        var loginBody = await login.Content.ReadFromJsonAsync<LoginResponse>(WebJson.Options);

        var refresh = await client.PostAsJsonAsync(
            "api/auth/refresh",
            new { refreshToken = loginBody!.RefreshToken },
            WebJson.Options);

        refresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshBody = await refresh.Content.ReadFromJsonAsync<LoginResponse>(WebJson.Options);
        refreshBody.Should().NotBeNull();
        refreshBody!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshBody.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken);
}

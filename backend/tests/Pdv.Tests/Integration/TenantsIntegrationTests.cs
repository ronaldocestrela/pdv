using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Pdv.Tests.Integration;

/// <summary>
/// Testes de integração HTTP para o fluxo de cadastro e gestão de tenants.
/// </summary>
public sealed class TenantsIntegrationTests
{
    // ─── Auto-registro público ────────────────────────────────────────────────

    [Fact]
    public async Task Register_returns_created_with_valid_data()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/tenants/register",
            new
            {
                name = "Empresa Teste Ltda",
                adminEmail = "admin@empresateste.com",
                adminPassword = "Senha@123",
            },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<IntegrationApiHelper.IdDto>(WebJson.Options);
        body.Should().NotBeNull();
        body!.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_returns_bad_request_when_password_too_short()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/tenants/register",
            new
            {
                name = "Loja XPTO",
                adminEmail = "admin@xpto.com",
                adminPassword = "123",
            },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_returns_bad_request_when_name_empty()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/tenants/register",
            new
            {
                name = "",
                adminEmail = "admin@loja.com",
                adminPassword = "Senha@123",
            },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_returns_conflict_when_name_already_taken()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var payload = new
        {
            name = "Loja Única",
            adminEmail = "admin1@lojaunica.com",
            adminPassword = "Senha@123",
        };

        var first = await client.PostAsJsonAsync("api/tenants/register", payload, WebJson.Options);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync(
            "api/tenants/register",
            new { name = "Loja Única", adminEmail = "outro@lojaunica.com", adminPassword = "Senha@123" },
            WebJson.Options);

        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_allows_tenant_admin_to_login_after_creation()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        const string tenantName = "Loja Nova Tenant";
        const string adminEmail = "admin@lovantenent.com";
        const string adminPassword = "Senha@123";

        var register = await client.PostAsJsonAsync(
            "api/tenants/register",
            new { name = tenantName, adminEmail, adminPassword },
            WebJson.Options);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email = adminEmail, password = adminPassword },
            WebJson.Options);

        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>(WebJson.Options);
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.TenantId.Should().BeGreaterThan(0);
    }

    // ─── Endpoint protegido (tenant.manage) ──────────────────────────────────

    [Fact]
    public async Task List_tenants_returns_forbidden_without_token()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_tenants_returns_ok_for_super_admin()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        // Cria um tenant para garantir lista não vazia
        var register = await client.PostAsJsonAsync(
            "api/tenants/register",
            new { name = "Tenant Listagem", adminEmail = "admin@listagem.com", adminPassword = "Senha@123" },
            WebJson.Options);
        register.StatusCode.Should().Be(HttpStatusCode.Created);

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var response = await client.GetAsync("api/tenants");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TenantRow[]>(WebJson.Options);
        body.Should().NotBeNull();
        body!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SetActive_returns_no_content_for_super_admin()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();
        await IntegrationApiHelper.LoginAsAdminAsync(client);

        // Cria um tenant para testar
        var register = await client.PostAsJsonAsync(
            "api/tenants/register",
            new { name = "Tenant Ativar", adminEmail = "admin@ativar.com", adminPassword = "Senha@123" },
            WebJson.Options);
        register.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await register.Content.ReadFromJsonAsync<IntegrationApiHelper.IdDto>(WebJson.Options);

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var deactivate = await client.PutAsJsonAsync(
            $"api/tenants/{created!.Id}/activate",
            new { isActive = false },
            WebJson.Options);

        deactivate.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken, int TenantId);

    private sealed record TenantRow(int Id, string Name, bool IsActive, string CreatedAtUtc);
}

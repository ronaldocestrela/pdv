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
        body!.Id.Should().NotBeEmpty();
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
        body.TenantId.Should().NotBeEmpty();
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

    // ─── Isolamento de Tenant (Roles e Usuários) ─────────────────────────────

    [Fact]
    public async Task Roles_and_Users_are_properly_isolated_between_tenants()
    {
        await using var factory = new PdvWebApplicationFactory();
        var clientA = factory.CreateClient();
        var clientB = factory.CreateClient();
        var clientC = factory.CreateClient();

        // 1. Cadastra o Tenant A (id = 1)
        var registerA = await clientA.PostAsJsonAsync(
            "api/tenants/register",
            new { name = "Tenant A", adminEmail = "admin@tenanta.com", adminPassword = "Senha@123" },
            WebJson.Options);
        registerA.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // 2. Cadastra o Tenant B (id = 2)
        var registerB = await clientB.PostAsJsonAsync(
            "api/tenants/register",
            new { name = "Tenant B", adminEmail = "admin@tenantb.com", adminPassword = "Senha@123" },
            WebJson.Options);
        registerB.StatusCode.Should().Be(HttpStatusCode.Created);

        // 3. Cadastra o Tenant C (id = 3)
        var registerC = await clientC.PostAsJsonAsync(
            "api/tenants/register",
            new { name = "Tenant C", adminEmail = "admin@tenantc.com", adminPassword = "Senha@123" },
            WebJson.Options);
        registerC.StatusCode.Should().Be(HttpStatusCode.Created);

        // 4. Fazer login como Tenant B Admin (id = 2)
        var loginBRes = await clientB.PostAsJsonAsync(
            "api/auth/login",
            new { email = "admin@tenantb.com", password = "Senha@123" },
            WebJson.Options);
        loginBRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenB = (await loginBRes.Content.ReadFromJsonAsync<LoginResponse>(WebJson.Options))!;
        clientB.SetBearer(tokenB.AccessToken);

        // 5. Fazer login como Tenant C Admin (id = 3)
        var loginCRes = await clientC.PostAsJsonAsync(
            "api/auth/login",
            new { email = "admin@tenantc.com", password = "Senha@123" },
            WebJson.Options);
        loginCRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenC = (await loginCRes.Content.ReadFromJsonAsync<LoginResponse>(WebJson.Options))!;
        clientC.SetBearer(tokenC.AccessToken);

        // 6. Listar usuários do Tenant B
        var usersB = await clientB.GetFromJsonAsync<UserRow[]>("api/users", WebJson.Options);
        usersB.Should().NotBeNull();
        usersB!.Should().ContainSingle(u => u.Email == "admin@tenantb.com");
        usersB.Should().NotContain(u => u.Email == "admin@tenanta.com");
        usersB.Should().NotContain(u => u.Email == "admin@tenantc.com");
        usersB.Should().NotContain(u => u.Email == "integration@local");

        // 7. Listar roles do Tenant B
        var rolesB = await clientB.GetFromJsonAsync<RoleRow[]>("api/roles", WebJson.Options);
        rolesB.Should().NotBeNull();
        rolesB!.Should().Contain(r => r.Name == "Super Admin");
        
        // Obter o ID da role Super Admin de C para tentar burlar
        var rolesC = await clientC.GetFromJsonAsync<RoleRow[]>("api/roles", WebJson.Options);
        var superAdminCId = rolesC!.First(r => r.Name == "Super Admin").Id;

        // 8. Tenant B tenta ler a role do Tenant C individualmente -> deve retornar 404
        var getRoleCResponse = await clientB.GetAsync($"api/roles/{superAdminCId}");
        getRoleCResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // 9. Tenant B tenta associar uma role do Tenant C a um de seus usuários -> deve falhar (400)
        var userBId = usersB.First(u => u.Email == "admin@tenantb.com").Id;
        var setRoleResponse = await clientB.PutAsJsonAsync(
            $"api/users/{userBId}/roles",
            new { roleIds = new[] { superAdminCId } },
            WebJson.Options);
        setRoleResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // 10. Tenant B tenta acessar gerenciamento de tenants -> deve retornar 403 Forbidden
        var manageTenantsRes = await clientB.GetAsync("api/tenants");
        manageTenantsRes.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private sealed record LoginResponse(string AccessToken, string RefreshToken, Guid TenantId);

    private sealed record TenantRow(Guid Id, string Name, bool IsActive, string CreatedAtUtc);

    private sealed record UserRow(Guid Id, string Email, bool IsActive, Guid[] RoleIds);

    private sealed record RoleRow(Guid Id, string Name, string[] Permissions);
}

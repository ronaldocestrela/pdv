using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Pdv.Tests.Integration;

/// <summary>In-memory API host with isolated EF Core database name per factory instance.</summary>
public sealed class PdvWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _inMemoryDatabaseName = Guid.NewGuid().ToString("N");

    public const string TestAdminEmail = "integration@local";
    public const string TestAdminPassword = "IntegrationTest123!";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("Database:UseInMemory", "true");
        builder.UseSetting("Database:InMemoryDatabaseName", _inMemoryDatabaseName);
        builder.UseSetting("Jwt:Key", "TestingOnlySymmetricKey_UseAtLeastThirtyTwoChars!");
        builder.UseSetting("Jwt:Issuer", "pdv-test");
        builder.UseSetting("Jwt:Audience", "pdv-test-web");
        builder.UseSetting("Jwt:AccessTokenMinutes", "60");
        builder.UseSetting("Jwt:RefreshTokenDays", "7");
        builder.UseSetting("Seed:SuperAdminEmail", TestAdminEmail);
        builder.UseSetting("Seed:SuperAdminPassword", TestAdminPassword);
    }
}

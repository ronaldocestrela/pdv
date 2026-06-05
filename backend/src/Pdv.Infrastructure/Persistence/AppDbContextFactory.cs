using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pdv.Infrastructure.Persistence;

using Pdv.Infrastructure.Services;

/// <summary>Allows <c>dotnet ef migrations</c> without resolving the full startup pipeline.</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates a new instance in the system.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Connection string only used to compile the model during design-time; migrations are provider-agnostic.
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=PdvMigr;Trusted_Connection=True;TrustServerCertificate=True");
        return new AppDbContext(optionsBuilder.Options, new SystemTenantContext());
    }
}

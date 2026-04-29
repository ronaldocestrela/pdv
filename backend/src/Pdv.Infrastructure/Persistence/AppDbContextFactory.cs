using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pdv.Infrastructure.Persistence;

/// <summary>Allows <c>dotnet ef migrations</c> without resolving the full startup pipeline.</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Connection string only used to compile the model during design-time; migrations are provider-agnostic.
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=PdvMigr;Trusted_Connection=True;TrustServerCertificate=True");
        return new AppDbContext(optionsBuilder.Options);
    }
}

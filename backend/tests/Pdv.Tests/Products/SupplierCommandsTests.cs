using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Catalog.Application.Commands.Suppliers;
using Pdv.Modules.Catalog.Application.Handlers.Suppliers;
using Pdv.Modules.Catalog.Domain.Entities;
using Pdv.Modules.Catalog.Infrastructure.Persistence;
using Pdv.Modules.Catalog.Infrastructure.Persistence.Repositories;
using Pdv.Shared.Kernel.Services;

namespace Pdv.Tests.Products;

/// <summary>
/// Unit tests for supplier CQRS commands and validators.
/// </summary>
public sealed class SupplierCommandsTests
{
    /// <summary>
    /// Verifies that creating a supplier correctly persists it in the database.
    /// </summary>
    [Fact]
    public async Task CreateSupplier_Persists()
    {
        // Arrange
        await using var ctx = NewDb();
        var repo = new CatalogRepository(ctx);
        var handler = new CreateSupplierCommandHandler(repo);

        // Act
        var id = await handler.Handle(
            new CreateSupplierCommand("Fornecedor A", "12.345.678/0001-90", "contato@a.com", "11999999999", true),
            CancellationToken.None);

        // Assert
        id.Should().NotBeEmpty();
        (await ctx.Suppliers.CountAsync()).Should().Be(1);
        
        var persisted = await ctx.Suppliers.FirstAsync();
        persisted.Name.Should().Be("Fornecedor A");
        persisted.Document.Should().Be("12.345.678/0001-90");
        persisted.Email.Should().Be("contato@a.com");
        persisted.Phone.Should().Be("11999999999");
        persisted.IsActive.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that creating a supplier with a duplicate document throws a ValidationException.
    /// </summary>
    [Fact]
    public async Task CreateSupplier_Rejects_DuplicateDocument()
    {
        // Arrange
        await using var ctx = NewDb();
        ctx.Suppliers.Add(new Supplier
        {
            Name = "Existing Supplier",
            Document = "12.345.678/0001-90",
            IsActive = true
        });
        await ctx.SaveChangesAsync();

        var repo = new CatalogRepository(ctx);
        var handler = new CreateSupplierCommandHandler(repo);

        // Act
        var act = async () => await handler.Handle(
            new CreateSupplierCommand("New Supplier", "12.345.678/0001-90", null, null, true),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Documento já cadastrado para outro fornecedor.*");
    }

    /// <summary>
    /// Verifies that updating a supplier modifies its properties.
    /// </summary>
    [Fact]
    public async Task UpdateSupplier_UpdatesFields()
    {
        // Arrange
        await using var ctx = NewDb();
        var supplier = new Supplier
        {
            Name = "Old Name",
            Document = "12.345.678/0001-90",
            IsActive = true
        };
        ctx.Suppliers.Add(supplier);
        await ctx.SaveChangesAsync();

        var repo = new CatalogRepository(ctx);
        var handler = new UpdateSupplierCommandHandler(repo);

        // Act
        await handler.Handle(
            new UpdateSupplierCommand(supplier.Id, "New Name", "98.765.432/0001-10", "new@email.com", "2288888888", false),
            CancellationToken.None);

        // Assert
        var updated = await ctx.Suppliers.FirstAsync();
        updated.Name.Should().Be("New Name");
        updated.Document.Should().Be("98.765.432/0001-10");
        updated.Email.Should().Be("new@email.com");
        updated.Phone.Should().Be("2288888888");
        updated.IsActive.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that deleting a supplier removes it from the database.
    /// </summary>
    [Fact]
    public async Task DeleteSupplier_Removes()
    {
        // Arrange
        await using var ctx = NewDb();
        var supplier = new Supplier
        {
            Name = "To Delete",
            IsActive = true
        };
        ctx.Suppliers.Add(supplier);
        await ctx.SaveChangesAsync();

        var repo = new CatalogRepository(ctx);
        var handler = new DeleteSupplierCommandHandler(repo);

        // Act
        await handler.Handle(new DeleteSupplierCommand(supplier.Id), CancellationToken.None);

        // Assert
        (await ctx.Suppliers.CountAsync()).Should().Be(0);
    }

    private static CatalogDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CatalogDbContext(options, new SystemTenantContext());
    }
}

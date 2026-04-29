using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pdv.Application.Commands.Products;
using Pdv.Application.Commands.Variations;
using Pdv.Application.Handlers.Products;
using Pdv.Application.Handlers.Variations;
using Pdv.Domain.Entities;
using Pdv.Infrastructure.Persistence;
using Pdv.Infrastructure.Repositories;

namespace Pdv.Tests.Products;

public sealed class ProductCommandsTests
{
    [Fact]
    public async Task CreateProduct_Persists()
    {
        await using var ctx = NewDb();
        var repo = new ProductRepository(ctx);
        var handler = new CreateProductCommandHandler(repo);

        var id = await handler.Handle(new CreateProductCommand("Camiseta", true), CancellationToken.None);

        id.Should().BeGreaterThan(0);
        (await ctx.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateVariation_Rejects_DuplicateBarcode()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "P", IsActive = true });
        await ctx.SaveChangesAsync();
        var productId = ctx.Products.First().Id;

        ctx.ProductVariations.Add(new ProductVariation
        {
            ProductId = productId,
            Name = "A",
            Barcode = "789",
            StockQuantity = 1,
        });
        await ctx.SaveChangesAsync();

        var repo = new ProductRepository(ctx);
        var handler = new CreateVariationCommandHandler(repo);

        var act = async () => await handler.Handle(
            new CreateVariationCommand(productId, "B", "789", 2),
            CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task DeleteProduct_CascadesVariations()
    {
        await using var ctx = NewDb();
        var p = new Product { Name = "P", IsActive = true };
        ctx.Products.Add(p);
        await ctx.SaveChangesAsync();
        ctx.ProductVariations.Add(new ProductVariation
        {
            ProductId = p.Id,
            Name = "V",
            Barcode = null,
            StockQuantity = 0,
        });
        await ctx.SaveChangesAsync();

        var repo = new ProductRepository(ctx);
        var handler = new DeleteProductCommandHandler(repo);
        await handler.Handle(new DeleteProductCommand(p.Id), CancellationToken.None);

        (await ctx.Products.CountAsync()).Should().Be(0);
        (await ctx.ProductVariations.CountAsync()).Should().Be(0);
    }

    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}

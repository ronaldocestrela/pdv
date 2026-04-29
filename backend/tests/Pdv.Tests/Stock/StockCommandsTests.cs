using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pdv.Application.Commands.Stock;
using Pdv.Application.Handlers.Stock;
using Pdv.Application.Queries.Stock;
using Pdv.Domain.Entities;
using Pdv.Domain.Enums;
using Pdv.Infrastructure.Persistence;
using Pdv.Infrastructure.Repositories;

namespace Pdv.Tests.Stock;

public sealed class StockCommandsTests
{
    [Fact]
    public async Task AddStock_IncrementsStock_AndCreatesInMovement()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "Camisa", IsActive = true });
        await ctx.SaveChangesAsync();
        var pid = ctx.Products.First().Id;
        ctx.ProductVariations.Add(new ProductVariation
        {
            ProductId = pid,
            Name = "P",
            Barcode = null,
            StockQuantity = 5,
            UnitPrice = 0,
        });
        await ctx.SaveChangesAsync();
        var vid = ctx.ProductVariations.First().Id;

        var repo = new ProductRepository(ctx);
        var handler = new AddStockCommandHandler(repo);

        await handler.Handle(new AddStockCommand(vid, 3, "Compra"), CancellationToken.None);

        var v = await ctx.ProductVariations.AsNoTracking().FirstAsync(x => x.Id == vid);
        v.StockQuantity.Should().Be(8);

        var m = await ctx.StockMovements.AsNoTracking().SingleAsync();
        m.Type.Should().Be(StockMovementType.In);
        m.Quantity.Should().Be(3);
        m.ProductVariationId.Should().Be(vid);
        m.Reason.Should().Be("Compra");
    }

    [Fact]
    public async Task AddStock_Throws_WhenVariationMissing()
    {
        await using var ctx = NewDb();
        var repo = new ProductRepository(ctx);
        var handler = new AddStockCommandHandler(repo);

        var act = async () => await handler.Handle(new AddStockCommand(999, 1, null), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task GetStockMovements_ReturnsOrderedRows()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "P", IsActive = true });
        await ctx.SaveChangesAsync();
        var pid = ctx.Products.First().Id;
        ctx.ProductVariations.Add(new ProductVariation
        {
            ProductId = pid,
            Name = "V",
            StockQuantity = 0,
            UnitPrice = 0,
        });
        await ctx.SaveChangesAsync();
        var vid = ctx.ProductVariations.First().Id;

        ctx.StockMovements.Add(new StockMovement
        {
            ProductVariationId = vid,
            Type = StockMovementType.In,
            Quantity = 2,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            Reason = "A",
        });
        ctx.StockMovements.Add(new StockMovement
        {
            ProductVariationId = vid,
            Type = StockMovementType.In,
            Quantity = 1,
            CreatedAtUtc = DateTime.UtcNow,
            Reason = "B",
        });
        await ctx.SaveChangesAsync();

        var repo = new ProductRepository(ctx);
        var handler = new GetStockMovementsQueryHandler(repo);

        var rows = await handler.Handle(new GetStockMovementsQuery(vid, 50), CancellationToken.None);

        rows.Should().HaveCount(2);
        rows[0].Quantity.Should().Be(1);
        rows[0].Type.Should().Be("IN");
        rows[1].Quantity.Should().Be(2);
    }

    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}

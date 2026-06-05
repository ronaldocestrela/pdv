using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pdv.Modules.Stock.Application.Commands.Stock;
using Pdv.Modules.Stock.Application.Handlers.Stock;
using Pdv.Modules.Stock.Application.Queries.Stock;
using Pdv.Modules.Stock.Domain.Entities;
using Pdv.Shared.Kernel.Enums;
using Pdv.Modules.Stock.Infrastructure.Persistence;
using Pdv.Modules.Stock.Infrastructure.Persistence.Repositories;

namespace Pdv.Tests.Stock;

public sealed class StockCommandsTests
{
    [Fact]
    public async Task AddStock_IncrementsStock_AndCreatesInMovement()
    {
        await using var ctx = NewDb();
        var product = new Product { Id = 1, Name = "Camisa" };
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        var variation = new ProductVariation
        {
            Id = 1,
            ProductId = 1,
            Product = product,
            Name = "P",
            StockQuantity = 5,
        };
        ctx.ProductVariations.Add(variation);
        await ctx.SaveChangesAsync();

        var repo = new StockRepository(ctx);
        var handler = new AddStockCommandHandler(repo);

        await handler.Handle(new AddStockCommand(1, 3, "Compra"), CancellationToken.None);

        var v = await ctx.ProductVariations.AsNoTracking().FirstAsync(x => x.Id == 1);
        v.StockQuantity.Should().Be(8);

        var m = await ctx.StockMovements.AsNoTracking().SingleAsync();
        m.Type.Should().Be(StockMovementType.In);
        m.Quantity.Should().Be(3);
        m.ProductVariationId.Should().Be(1);
        m.Reason.Should().Be("Compra");
    }

    [Fact]
    public async Task AddStock_Throws_WhenVariationMissing()
    {
        await using var ctx = NewDb();
        var repo = new StockRepository(ctx);
        var handler = new AddStockCommandHandler(repo);

        var act = async () => await handler.Handle(new AddStockCommand(999, 1, null), CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact]
    public async Task GetStockMovements_ReturnsOrderedRows()
    {
        await using var ctx = NewDb();
        var product = new Product { Id = 1, Name = "P" };
        ctx.Products.Add(product);
        await ctx.SaveChangesAsync();

        var variation = new ProductVariation
        {
            Id = 1,
            ProductId = 1,
            Product = product,
            Name = "V",
            StockQuantity = 0,
        };
        ctx.ProductVariations.Add(variation);
        await ctx.SaveChangesAsync();

        ctx.StockMovements.Add(new StockMovement
        {
            ProductVariationId = 1,
            Type = StockMovementType.In,
            Quantity = 2,
            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
            Reason = "A",
        });
        ctx.StockMovements.Add(new StockMovement
        {
            ProductVariationId = 1,
            Type = StockMovementType.In,
            Quantity = 1,
            CreatedAtUtc = DateTime.UtcNow,
            Reason = "B",
        });
        await ctx.SaveChangesAsync();

        var repo = new StockRepository(ctx);
        var handler = new GetStockMovementsQueryHandler(repo);

        var rows = await handler.Handle(new GetStockMovementsQuery(1, 50), CancellationToken.None);

        rows.Should().HaveCount(2);
        rows[0].Quantity.Should().Be(1);
        rows[0].Type.Should().Be("IN");
        rows[1].Quantity.Should().Be(2);
    }

    private static StockDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new StockDbContext(options, new Pdv.Shared.Kernel.Services.SystemTenantContext());
    }
}

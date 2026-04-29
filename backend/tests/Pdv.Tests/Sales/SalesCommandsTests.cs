using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Pdv.Application.Commands.Sales;
using Pdv.Application.Handlers.Sales;
using Pdv.Domain.Entities;
using Pdv.Domain.Enums;
using Pdv.Infrastructure.Persistence;
using Pdv.Infrastructure.Repositories;

namespace Pdv.Tests.Sales;

public sealed class SalesCommandsTests
{
    [Fact]
    public async Task CreateSale_DecrementsStock_CreatesOutMovement_AndCashFlowIn()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "Camisa", IsActive = true });
        await ctx.SaveChangesAsync();
        var pid = ctx.Products.First().Id;
        ctx.ProductVariations.Add(new ProductVariation
        {
            ProductId = pid,
            Name = "P",
            StockQuantity = 10,
            UnitPrice = 12.50m,
        });
        await ctx.SaveChangesAsync();
        var vid = ctx.ProductVariations.First().Id;

        var products = new ProductRepository(ctx);
        var sales = new SaleRepository(ctx);
        var handler = new CreateSaleCommandHandler(products, sales);

        var result = await handler.Handle(
            new CreateSaleCommand(
                [new CreateSaleLineDto(vid, 3)],
                PaymentMethod.Cash),
            CancellationToken.None);

        result.SaleId.Should().BeGreaterThan(0);
        result.TotalAmount.Should().Be(37.50m);

        var v = await ctx.ProductVariations.AsNoTracking().FirstAsync(x => x.Id == vid);
        v.StockQuantity.Should().Be(7);

        var movements = await ctx.StockMovements.AsNoTracking().ToListAsync();
        movements.Should().ContainSingle();
        movements[0].Type.Should().Be(StockMovementType.Out);
        movements[0].Quantity.Should().Be(3);

        var flows = await ctx.CashFlows.AsNoTracking().ToListAsync();
        flows.Should().ContainSingle();
        flows[0].Type.Should().Be(CashFlowType.In);
        flows[0].Amount.Should().Be(37.50m);
        flows[0].SaleId.Should().Be(result.SaleId);

        var items = await ctx.SaleItems.AsNoTracking().ToListAsync();
        items.Should().ContainSingle();
        items[0].Quantity.Should().Be(3);
        items[0].UnitPrice.Should().Be(12.50m);
    }

    [Fact]
    public async Task CreateSale_Throws_WhenInsufficientStock()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "P", IsActive = true });
        await ctx.SaveChangesAsync();
        var pid = ctx.Products.First().Id;
        ctx.ProductVariations.Add(new ProductVariation { ProductId = pid, Name = "V", StockQuantity = 1, UnitPrice = 10m });
        await ctx.SaveChangesAsync();
        var vid = ctx.ProductVariations.First().Id;

        var handler = new CreateSaleCommandHandler(new ProductRepository(ctx), new SaleRepository(ctx));

        var act = async () => await handler.Handle(
            new CreateSaleCommand([new CreateSaleLineDto(vid, 5)], PaymentMethod.Pix),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateSale_MergesSameVariationLines()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "P", IsActive = true });
        await ctx.SaveChangesAsync();
        var pid = ctx.Products.First().Id;
        ctx.ProductVariations.Add(new ProductVariation { ProductId = pid, Name = "V", StockQuantity = 10, UnitPrice = 10m });
        await ctx.SaveChangesAsync();
        var vid = ctx.ProductVariations.First().Id;

        var handler = new CreateSaleCommandHandler(new ProductRepository(ctx), new SaleRepository(ctx));

        await handler.Handle(
            new CreateSaleCommand(
                [
                    new CreateSaleLineDto(vid, 2),
                    new CreateSaleLineDto(vid, 1),
                ],
                PaymentMethod.Card),
            CancellationToken.None);

        var v = await ctx.ProductVariations.AsNoTracking().FirstAsync(x => x.Id == vid);
        v.StockQuantity.Should().Be(7);

        var items = await ctx.SaleItems.AsNoTracking().ToListAsync();
        items.Should().ContainSingle();
        items[0].Quantity.Should().Be(3);
    }

    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}

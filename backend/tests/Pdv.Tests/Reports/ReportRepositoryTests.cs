using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pdv.Domain.Entities;
using Pdv.Domain.Enums;
using Pdv.Infrastructure.Persistence;
using Pdv.Infrastructure.Repositories;

namespace Pdv.Tests.Reports;

public sealed class ReportRepositoryTests
{
    [Fact]
    public async Task GetSalesSummary_SumsSales_InclusiveRange()
    {
        await using var ctx = NewDb();
        var mid = new DateTime(2026, 4, 15, 12, 0, 0, DateTimeKind.Utc);
        ctx.Sales.Add(new Sale { CreatedAtUtc = mid.AddDays(-1), TotalAmount = 10m, PaymentMethod = PaymentMethod.Cash, Items = [] });
        ctx.Sales.Add(new Sale { CreatedAtUtc = mid, TotalAmount = 20m, PaymentMethod = PaymentMethod.Card, Items = [] });
        ctx.Sales.Add(new Sale { CreatedAtUtc = mid.AddDays(1), TotalAmount = 99m, PaymentMethod = PaymentMethod.Pix, Items = [] });
        await ctx.SaveChangesAsync();

        var sut = new ReportRepository(ctx);
        var from = new DateTime(2026, 4, 14, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 4, 15, 23, 59, 59, DateTimeKind.Utc);

        var r = await sut.GetSalesSummaryAsync(from, to);

        r.SaleCount.Should().Be(2);
        r.TotalAmount.Should().Be(30m);
    }

    [Fact]
    public async Task GetTopProducts_AggregatesByVariation_OrderByQtyDesc()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "Camisa", IsActive = true });
        await ctx.SaveChangesAsync();
        var pid = ctx.Products.First().Id;

        ctx.ProductVariations.Add(new ProductVariation { ProductId = pid, Name = "P", StockQuantity = 10, UnitPrice = 5m });
        ctx.ProductVariations.Add(new ProductVariation { ProductId = pid, Name = "M", StockQuantity = 10, UnitPrice = 10m });
        await ctx.SaveChangesAsync();

        var vP = ctx.ProductVariations.First(v => v.Name == "P");
        var vM = ctx.ProductVariations.First(v => v.Name == "M");

        var saleDay = new DateTime(2026, 4, 10, 10, 0, 0, DateTimeKind.Utc);
        ctx.Sales.Add(new Sale
        {
            CreatedAtUtc = saleDay,
            TotalAmount = 40m,
            PaymentMethod = PaymentMethod.Cash,
            Items =
            [
                new SaleItem { ProductVariationId = vP.Id, Quantity = 2, UnitPrice = 5m },
                new SaleItem { ProductVariationId = vM.Id, Quantity = 3, UnitPrice = 10m },
            ],
        });
        await ctx.SaveChangesAsync();

        var sut = new ReportRepository(ctx);
        var from = saleDay.Date;
        var to = saleDay.Date.AddDays(1).AddTicks(-1);

        var rows = await sut.GetTopProductsAsync(from, to, 10);

        rows.Should().HaveCount(2);
        rows[0].VariationName.Should().Be("M");
        rows[0].QuantitySold.Should().Be(3);
        rows[0].Revenue.Should().Be(30m);
        rows[1].VariationName.Should().Be("P");
        rows[1].QuantitySold.Should().Be(2);
        rows[1].Revenue.Should().Be(10m);
    }

    [Fact]
    public async Task ListCashFlows_FiltersByPeriod_OrderDesc()
    {
        await using var ctx = NewDb();
        var d = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc);
        ctx.CashFlows.Add(new CashFlow { Type = CashFlowType.In, Amount = 100m, Description = "A", CreatedAtUtc = d.AddHours(-2), SaleId = null });
        ctx.CashFlows.Add(new CashFlow { Type = CashFlowType.Out, Amount = 10m, Description = "B", CreatedAtUtc = d, SaleId = 1 });
        await ctx.SaveChangesAsync();

        var sut = new ReportRepository(ctx);
        var rows = await sut.ListCashFlowsAsync(d.Date, d.Date.AddDays(1).AddTicks(-1), 50);

        rows.Should().HaveCount(2);
        rows[0].Description.Should().Be("B");
        rows[1].Description.Should().Be("A");
    }

    [Fact]
    public async Task ListStockLevels_OrdersByProductThenVariation()
    {
        await using var ctx = NewDb();
        ctx.Products.Add(new Product { Name = "Zebra", IsActive = true });
        ctx.Products.Add(new Product { Name = "Alpha", IsActive = true });
        await ctx.SaveChangesAsync();

        var zebraId = ctx.Products.First(p => p.Name == "Zebra").Id;
        var alphaId = ctx.Products.First(p => p.Name == "Alpha").Id;

        ctx.ProductVariations.Add(new ProductVariation { ProductId = zebraId, Name = "Único", StockQuantity = 1, UnitPrice = 1m });
        ctx.ProductVariations.Add(new ProductVariation { ProductId = alphaId, Name = "B", StockQuantity = 2, UnitPrice = 1m });
        ctx.ProductVariations.Add(new ProductVariation { ProductId = alphaId, Name = "A", StockQuantity = 3, UnitPrice = 1m });
        await ctx.SaveChangesAsync();

        var sut = new ReportRepository(ctx);
        var rows = await sut.ListStockLevelsAsync(500);

        rows.Select(r => r.ProductName).Should().ContainInOrder("Alpha", "Alpha", "Zebra");
        rows.Where(r => r.ProductName == "Alpha").Select(r => r.VariationName).Should().ContainInOrder("A", "B");
    }

    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}

using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pdv.Modules.Sales.Application.Commands.Sales;
using Pdv.Modules.Sales.Application.Handlers.Sales;
using Pdv.Modules.Sales.Domain.Entities;
using Pdv.Modules.Sales.Infrastructure.Persistence;
using Pdv.Modules.Sales.Infrastructure.Persistence.Repositories;
using Pdv.Shared.Kernel.Enums;
using Pdv.Shared.Kernel.Events;

namespace Pdv.Tests.Sales;

public sealed class SalesCommandsTests
{
    [Fact]
    public async Task CreateSale_DecrementsStock_CreatesOutMovement_AndCashFlowIn()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var salesCtx = NewSalesDb(dbName);
        await using var stockCtx = NewStockDb(dbName);

        // Seed Sales context variation
        var salesVariation = new ProductVariation
        {
            Id = 1,
            Name = "P",
            StockQuantity = 10,
            UnitPrice = 12.50m
        };
        salesCtx.ProductVariations.Add(salesVariation);
        await salesCtx.SaveChangesAsync();

        // Seed Stock context product and variation
        var stockProduct = new Pdv.Modules.Stock.Domain.Entities.Product { Id = 1, Name = "P" };
        var stockVariation = new Pdv.Modules.Stock.Domain.Entities.ProductVariation
        {
            Id = 1,
            ProductId = 1,
            Product = stockProduct,
            Name = "P",
            StockQuantity = 10
        };
        stockCtx.Products.Add(stockProduct);
        stockCtx.ProductVariations.Add(stockVariation);
        await stockCtx.SaveChangesAsync();

        var salesRepo = new SalesRepository(salesCtx);
        
        var publisherMock = new Mock<IPublisher>();
        SaleFinalizedIntegrationEvent? publishedEvent = null;
        publisherMock.Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((ev, ct) => {
                if (ev is SaleFinalizedIntegrationEvent se)
                    publishedEvent = se;
            })
            .Returns(Task.CompletedTask);

        var handler = new CreateSaleCommandHandler(salesRepo, publisherMock.Object);

        var result = await handler.Handle(
            new CreateSaleCommand(
                [new CreateSaleLineDto(1, 3)],
                PaymentMethod.Cash),
            CancellationToken.None);

        result.SaleId.Should().BeGreaterThan(0);
        result.TotalAmount.Should().Be(37.50m);

        var flows = await salesCtx.CashFlows.AsNoTracking().ToListAsync();
        flows.Should().ContainSingle();
        flows[0].Type.Should().Be(CashFlowType.In);
        flows[0].Amount.Should().Be(37.50m);
        flows[0].SaleId.Should().Be(result.SaleId);

        var items = await salesCtx.SaleItems.AsNoTracking().ToListAsync();
        items.Should().ContainSingle();
        items[0].Quantity.Should().Be(3);
        items[0].UnitPrice.Should().Be(12.50m);

        publishedEvent.Should().NotBeNull();
        publishedEvent!.SaleId.Should().Be(result.SaleId);
        publishedEvent.Items.Should().ContainSingle(i => i.ProductVariationId == 1 && i.Quantity == 3);

        // Manually process the integration event using Stock's handler to simulate the integration flow
        var stockRepo = new Pdv.Modules.Stock.Infrastructure.Persistence.Repositories.StockRepository(stockCtx);
        var stockHandler = new Pdv.Modules.Stock.Application.Handlers.Integration.DecrementStockOnSaleFinalized(stockRepo);
        await stockHandler.Handle(publishedEvent, CancellationToken.None);

        var v = await stockCtx.ProductVariations.AsNoTracking().FirstAsync(x => x.Id == 1);
        v.StockQuantity.Should().Be(7);

        var movements = await stockCtx.StockMovements.AsNoTracking().ToListAsync();
        movements.Should().ContainSingle();
        movements[0].Type.Should().Be(StockMovementType.Out);
        movements[0].Quantity.Should().Be(3);
    }

    [Fact]
    public async Task CreateSale_Throws_WhenInsufficientStock()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var salesCtx = NewSalesDb(dbName);
        var variation = new ProductVariation { Id = 1, Name = "V", StockQuantity = 1, UnitPrice = 10m };
        salesCtx.ProductVariations.Add(variation);
        await salesCtx.SaveChangesAsync();

        var handler = new CreateSaleCommandHandler(new SalesRepository(salesCtx), Mock.Of<IPublisher>());

        var act = async () => await handler.Handle(
            new CreateSaleCommand([new CreateSaleLineDto(1, 5)], PaymentMethod.Pix),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateSale_MergesSameVariationLines()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var salesCtx = NewSalesDb(dbName);
        await using var stockCtx = NewStockDb(dbName);

        // Seed Sales context variation
        var salesVariation = new ProductVariation { Id = 1, Name = "V", StockQuantity = 10, UnitPrice = 10m };
        salesCtx.ProductVariations.Add(salesVariation);
        await salesCtx.SaveChangesAsync();

        // Seed Stock context product and variation
        var stockProduct = new Pdv.Modules.Stock.Domain.Entities.Product { Id = 1, Name = "V" };
        var stockVariation = new Pdv.Modules.Stock.Domain.Entities.ProductVariation
        {
            Id = 1,
            ProductId = 1,
            Product = stockProduct,
            Name = "V",
            StockQuantity = 10
        };
        stockCtx.Products.Add(stockProduct);
        stockCtx.ProductVariations.Add(stockVariation);
        await stockCtx.SaveChangesAsync();

        var salesRepo = new SalesRepository(salesCtx);

        var publisherMock = new Mock<IPublisher>();
        SaleFinalizedIntegrationEvent? publishedEvent = null;
        publisherMock.Setup(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((ev, ct) => {
                if (ev is SaleFinalizedIntegrationEvent se)
                    publishedEvent = se;
            })
            .Returns(Task.CompletedTask);

        var handler = new CreateSaleCommandHandler(salesRepo, publisherMock.Object);

        await handler.Handle(
            new CreateSaleCommand(
                [
                    new CreateSaleLineDto(1, 2),
                    new CreateSaleLineDto(1, 1),
                ],
                PaymentMethod.Card),
            CancellationToken.None);

        var items = await salesCtx.SaleItems.AsNoTracking().ToListAsync();
        items.Should().ContainSingle();
        items[0].Quantity.Should().Be(3);

        publishedEvent.Should().NotBeNull();
        publishedEvent!.Items.Should().ContainSingle(i => i.ProductVariationId == 1 && i.Quantity == 3);

        // Manually process the integration event using Stock's handler to simulate the integration flow
        var stockRepo = new Pdv.Modules.Stock.Infrastructure.Persistence.Repositories.StockRepository(stockCtx);
        var stockHandler = new Pdv.Modules.Stock.Application.Handlers.Integration.DecrementStockOnSaleFinalized(stockRepo);
        await stockHandler.Handle(publishedEvent, CancellationToken.None);

        var v = await stockCtx.ProductVariations.AsNoTracking().FirstAsync(x => x.Id == 1);
        v.StockQuantity.Should().Be(7);
    }

    private static SalesDbContext NewSalesDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new SalesDbContext(options, new Pdv.Shared.Kernel.Services.SystemTenantContext());
    }

    private static Pdv.Modules.Stock.Infrastructure.Persistence.StockDbContext NewStockDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<Pdv.Modules.Stock.Infrastructure.Persistence.StockDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new Pdv.Modules.Stock.Infrastructure.Persistence.StockDbContext(options, new Pdv.Shared.Kernel.Services.SystemTenantContext());
    }
}

using System.Net;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Pdv.Web.Auth;
using Pdv.Web.Models;
using Pdv.Web.Pages;
using Xunit;

namespace Pdv.Web.Tests.Pages;

public class PdvPageTests : BlazorTestBase
{
    private void SetupAuthenticatedUser()
    {
        var session = new AuthSession
        {
            AccessToken = "valid-token",
            Email = "admin@local",
            UserId = "user-123",
            Permissions = ["sale.create", "sale.view", "product.view"]
        };

        MockLocalStorage.Setup(x => x.GetItemAsync<AuthSession>("pdv-auth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        AuthProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public async Task PdvPage_InitialRender_ShouldLoadCatalogAndSales()
    {
        // Arrange
        SetupAuthenticatedUser();

        var products = new List<ProductSummaryDto>
        {
            new() { Id = "p-1", Name = "Camiseta", VariationCount = 2 },
            new() { Id = "p-2", Name = "Calça", VariationCount = 1 }
        };

        var sales = new List<SaleListItemDto>
        {
            new() { Id = "s-1", CreatedAtUtc = DateTime.UtcNow.ToString("o"), ItemCount = 1, TotalAmount = 50.00m, PaymentMethod = "pix" }
        };

        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, products);
        HttpHandler.Setup("GET", "/api/sales", HttpStatusCode.OK, sales);

        // Act
        var cut = RenderComponent<PdvPage>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando…"));

        // Assert - Catalog dropdown filled
        var productOptions = cut.FindAll("select#pdv-product option");
        productOptions.Should().HaveCount(3); // "Selecione..." + 2 products
        productOptions[1].TextContent.Should().Be("Camiseta");
        productOptions[2].TextContent.Should().Be("Calça");

        // Assert - Recent sales rendered
        var recentSalesCard = cut.FindAll("div.pdv-card")
            .First(card => card.QuerySelector("h2")?.TextContent.Contains("Últimas vendas") == true);
        var salesRows = recentSalesCard.QuerySelectorAll("tbody tr");
        salesRows.Should().NotBeEmpty();
        salesRows[0].TextContent.Should().Contain("R$ 50,00");
        salesRows[0].TextContent.Should().Contain("PIX");
    }

    [Fact]
    public async Task PdvPage_AddProductToCart_ShouldUpdateCartAndTotal()
    {
        // Arrange
        SetupAuthenticatedUser();

        var products = new List<ProductSummaryDto>
        {
            new() { Id = "p-1", Name = "Camiseta", VariationCount = 1 }
        };

        var productDetail = new ProductDetailDto
        {
            Id = "p-1",
            Name = "Camiseta",
            IsActive = true,
            Variations = new List<ProductVariationDto>
            {
                new() { Id = "v-1", Name = "Preta G", Barcode = "123456", StockQuantity = 10, UnitPrice = 45.00m }
            }
        };

        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, products);
        HttpHandler.Setup("GET", "/api/sales", HttpStatusCode.OK, new List<SaleListItemDto>());
        HttpHandler.Setup("GET", "/api/products/p-1", HttpStatusCode.OK, productDetail);

        var cut = RenderComponent<PdvPage>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando…"));

        // Act - Select Product from dropdown
        var productSelect = cut.Find("select#pdv-product");
        productSelect.Change("p-1");

        // Wait for variations to load
        cut.WaitForState(() => cut.FindAll("select#pdv-variation option").Count > 1);

        // Act - Select Variation G
        var variationSelect = cut.Find("select#pdv-variation");
        variationSelect.Change("v-1");

        // Verify price and stock info loaded
        cut.Find("#pdv-price-display").TextContent.Should().Contain("R$ 45,00");
        var stockParagraph = cut.FindAll("p").First(p => p.TextContent.Contains("Estoque disponível"));
        stockParagraph.TextContent.Should().Contain("Estoque disponível: 10");

        // Act - Click Add to Cart
        var addBtn = cut.Find("button.pdv-btn--primary");
        addBtn.Click();

        // Assert - Cart has 1 line with correct item, subtotal and total
        var cartCard = cut.FindAll("div.pdv-card")
            .First(card => card.QuerySelector("h2")?.TextContent.Contains("Carrinho") == true);
        var cartRows = cartCard.QuerySelectorAll("tbody tr");
        cartRows.Should().HaveCount(1);
        cartRows[0].TextContent.Should().Contain("Camiseta");
        cartRows[0].TextContent.Should().Contain("Preta G");
        cartRows[0].TextContent.Should().Contain("R$ 45,00");

        var cartTotalSpan = cut.Find("span[style*='font-size:1.35rem']");
        cartTotalSpan.TextContent.Should().Be("R$ 45,00");
    }

    [Fact]
    public async Task PdvPage_FinalizeSale_ShouldSendApiAndClearCart()
    {
        // Arrange
        SetupAuthenticatedUser();

        // Setup product with variation
        var products = new List<ProductSummaryDto> { new() { Id = "p-1", Name = "Camiseta", VariationCount = 1 } };
        var productDetail = new ProductDetailDto
        {
            Id = "p-1", Name = "Camiseta", IsActive = true,
            Variations = [ new() { Id = "v-1", Name = "M", StockQuantity = 5, UnitPrice = 30.00m } ]
        };

        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, products);
        HttpHandler.Setup("GET", "/api/sales", HttpStatusCode.OK, new List<SaleListItemDto>());
        HttpHandler.Setup("GET", "/api/products/p-1", HttpStatusCode.OK, productDetail);

        var cut = RenderComponent<PdvPage>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando…"));

        // Select product and variation
        cut.Find("select#pdv-product").Change("p-1");
        cut.WaitForState(() => cut.FindAll("select#pdv-variation option").Count > 1);
        cut.Find("select#pdv-variation").Change("v-1");

        // Add to cart
        cut.Find("button.pdv-btn--primary").Click();

        // Mock checkout API endpoint
        var saleResult = new CreateSaleResultDto { SaleId = "sale-777", TotalAmount = 30.00m };
        HttpHandler.Setup("POST", "/api/sales", HttpStatusCode.OK, saleResult);

        // Expect reload of catalog and sales list
        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, products);
        var updatedSales = new List<SaleListItemDto>
        {
            new() { Id = "sale-777", CreatedAtUtc = DateTime.UtcNow.ToString("o"), ItemCount = 1, TotalAmount = 30.00m, PaymentMethod = "card" }
        };
        HttpHandler.Setup("GET", "/api/sales", HttpStatusCode.OK, updatedSales);

        // Select payment card
        var cardBtn = cut.FindAll("button.pdv-btn").First(b => b.TextContent.Contains("Cartão"));
        cardBtn.Click();

        // Act - Click checkout button
        var checkoutBtn = cut.FindAll("button.pdv-btn--primary").First(b => b.TextContent.Contains("Finalizar venda"));
        checkoutBtn.Click();

        // Assert - Notice message contains sale ID and cart is empty
        cut.WaitForState(() => cut.Find("p.pdv-empty").TextContent.Contains("Venda #sale-777 registrada"));
        cut.Find("p.pdv-empty").TextContent.Should().Contain("R$ 30,00");
        cut.Markup.Should().Contain("Carrinho vazio.");
    }
}

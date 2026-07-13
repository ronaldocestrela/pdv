using System.Net;
using System.Security.Claims;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using Pdv.Web.Auth;
using Pdv.Web.Models;
using Pdv.Web.Pages;
using Xunit;

namespace Pdv.Web.Tests.Pages;

public class ProductsTests : BlazorTestBase
{
    private void SetupAuthenticatedUser(params string[] permissions)
    {
        var session = new AuthSession
        {
            AccessToken = "valid-token",
            Email = "admin@local",
            UserId = "user-123",
            Permissions = permissions.ToList()
        };

        MockLocalStorage.Setup(x => x.GetItemAsync<AuthSession>("pdv-auth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Notify state changed so [Authorize] attribute passes
        AuthProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
    }

    [Fact]
    public void Products_InitialRender_WhenLoading_ShouldShowLoadingText()
    {
        // Arrange
        SetupAuthenticatedUser("product.view");
        
        // Setup simulated delay for loading state
        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, new List<ProductSummaryDto>(), delayMs: 2000);

        // Act
        var cut = RenderComponent<Products>();

        // Assert
        cut.Find(".pdv-empty").TextContent.Should().Be("Carregando…");
    }

    [Fact]
    public async Task Products_WithData_ShouldRenderProductTable()
    {
        // Arrange
        SetupAuthenticatedUser("product.view");

        var products = new List<ProductSummaryDto>
        {
            new() { Id = "p-1", Name = "Camiseta Preta", IsActive = true, VariationCount = 2 },
            new() { Id = "p-2", Name = "Calça Jeans", IsActive = false, VariationCount = 1 }
        };

        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, products);

        // Act
        var cut = RenderComponent<Products>();
        
        // Wait for asynchronous data loading
        cut.WaitForState(() => !cut.Markup.Contains("Carregando…"));

        // Assert
        var rows = cut.FindAll("table.pdv-table tbody tr");
        rows.Should().HaveCount(2);

        rows[0].TextContent.Should().Contain("Camiseta Preta");
        rows[0].TextContent.Should().Contain("Sim"); // IsActive = true
        rows[0].TextContent.Should().Contain("2"); // VariationCount = 2

        rows[1].TextContent.Should().Contain("Calça Jeans");
        rows[1].TextContent.Should().Contain("Não"); // IsActive = false
        rows[1].TextContent.Should().Contain("1"); // VariationCount = 1
    }

    [Fact]
    public async Task Products_CreateProduct_ShouldTriggerApiAndReload()
    {
        // Arrange
        SetupAuthenticatedUser("product.view", "product.create");

        // Initial empty product list
        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, new List<ProductSummaryDto>());
        
        var cut = RenderComponent<Products>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando…"));

        // Verify "Novo produto" button is present and click it
        var newProductBtn = cut.Find("button.pdv-btn--primary");
        newProductBtn.Click();

        // Verify modal opened
        cut.Find("h2#product-modal-title").TextContent.Should().Be("Novo produto");

        // Fill modal fields
        cut.Find("input#p-name").Change("Novo Tênis de Corrida");

        // Mock product creation endpoint response
        HttpHandler.Setup("POST", "/api/products", HttpStatusCode.OK, new { Id = "p-new" });

        // Mock list products endpoint again after reload
        var updatedProductsList = new List<ProductSummaryDto>
        {
            new() { Id = "p-new", Name = "Novo Tênis de Corrida", IsActive = true, VariationCount = 0 }
        };
        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, updatedProductsList);

        // Act - submit modal form
        await cut.InvokeAsync(() => cut.Find("form").Submit());

        // Assert - modal closed
        cut.FindAll("div.pdv-modal-overlay").Should().BeEmpty();

        // Assert - table loaded with new product
        cut.WaitForState(() => cut.Markup.Contains("Novo Tênis de Corrida"));
        cut.Find("table.pdv-table tbody tr").TextContent.Should().Contain("Novo Tênis de Corrida");
    }

    [Fact]
    public async Task Products_DeleteProduct_ShouldTriggerDeleteApiAndReload()
    {
        // Arrange
        SetupAuthenticatedUser("product.view", "product.delete");

        var products = new List<ProductSummaryDto>
        {
            new() { Id = "p-1", Name = "Produto a Excluir", IsActive = true, VariationCount = 0 }
        };

        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, products);

        var cut = RenderComponent<Products>();
        cut.WaitForState(() => !cut.Markup.Contains("Carregando…"));

        // Click delete button
        var deleteBtn = cut.Find("button.pdv-btn--danger");
        deleteBtn.Click();

        // Verify confirm delete modal opened
        cut.Find("div.pdv-modal h2").TextContent.Should().Be("Excluir produto");
        cut.Find("div.pdv-modal p").TextContent.Should().Contain("Produto a Excluir");

        // Setup mock delete API and empty reload response
        HttpHandler.Setup("DELETE", "/api/products/p-1", HttpStatusCode.OK);
        HttpHandler.Setup("GET", "/api/products", HttpStatusCode.OK, new List<ProductSummaryDto>());

        // Act - Confirm delete button click
        var confirmBtn = cut.Find("div.pdv-modal .pdv-btn--danger");
        confirmBtn.Click();

        // Assert - modal closed and empty state displayed
        cut.WaitForState(() => cut.Markup.Contains("Nenhum produto cadastrado."));
        cut.Find(".pdv-empty").TextContent.Should().Be("Nenhum produto cadastrado.");
    }
}

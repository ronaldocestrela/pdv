using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Pdv.Modules.Identity.Application.Auth;

namespace Pdv.Tests.Integration;

internal static class IntegrationApiHelper
{
    internal sealed record IdDto(Guid Id);

    public static void SetBearer(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public static void ClearBearer(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    public static async Task<TokenResponseDto> LoginAsAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "api/auth/login",
            new { email = PdvWebApplicationFactory.TestAdminEmail, password = PdvWebApplicationFactory.TestAdminPassword },
            WebJson.Options);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponseDto>(WebJson.Options);
        body.Should().NotBeNull();
        client.SetBearer(body!.AccessToken);
        return body;
    }

    public static async Task<Guid> CreateProductAndVariationAsync(
        HttpClient authorizedClient,
        int stockQuantity,
        decimal unitPrice = 12.50m)
    {
        var productRes = await authorizedClient.PostAsJsonAsync(
            "api/products",
            new { name = "Produto integração", isActive = true },
            WebJson.Options);
        productRes.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        var productId = (await productRes.Content.ReadFromJsonAsync<IdDto>(WebJson.Options))!.Id;

        var variationRes = await authorizedClient.PostAsJsonAsync(
            "api/variations",
            new
            {
                productId,
                name = "Variação única",
                barcode = (string?)null,
                stockQuantity,
                unitPrice,
            },
            WebJson.Options);

        variationRes.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        return (await variationRes.Content.ReadFromJsonAsync<IdDto>(WebJson.Options))!.Id;
    }

    public static async Task SyncVariationAsync(WebApplicationFactory<Program> factory, Guid variationId)
    {
        using var scope = factory.Services.CreateScope();
        var catalogDb = scope.ServiceProvider.GetRequiredService<Pdv.Modules.Catalog.Infrastructure.Persistence.CatalogDbContext>();
        var salesDb = scope.ServiceProvider.GetRequiredService<Pdv.Modules.Sales.Infrastructure.Persistence.SalesDbContext>();
        var stockDb = scope.ServiceProvider.GetRequiredService<Pdv.Modules.Stock.Infrastructure.Persistence.StockDbContext>();

        var catVar = await catalogDb.ProductVariations.Include(v => v.Product).FirstOrDefaultAsync(v => v.Id == variationId);
        if (catVar is null) return;

        // Sync to SalesDb
        if (!await salesDb.ProductVariations.AnyAsync(v => v.Id == variationId))
        {
            salesDb.ProductVariations.Add(new Pdv.Modules.Sales.Domain.Entities.ProductVariation
            {
                Id = catVar.Id,
                Name = catVar.Name,
                UnitPrice = catVar.UnitPrice,
                StockQuantity = catVar.StockQuantity
            });
            await salesDb.SaveChangesAsync();
        }

        // Sync to StockDb
        if (!await stockDb.ProductVariations.AnyAsync(v => v.Id == variationId))
        {
            var stockProd = await stockDb.Products.FindAsync(catVar.ProductId);
            if (stockProd is null)
            {
                stockProd = new Pdv.Modules.Stock.Domain.Entities.Product
                {
                    Id = catVar.ProductId,
                    Name = catVar.Product.Name
                };
                stockDb.Products.Add(stockProd);
                await stockDb.SaveChangesAsync();
            }

            stockDb.ProductVariations.Add(new Pdv.Modules.Stock.Domain.Entities.ProductVariation
            {
                Id = catVar.Id,
                ProductId = catVar.ProductId,
                Product = stockProd,
                Name = catVar.Name,
                StockQuantity = catVar.StockQuantity
            });
            await stockDb.SaveChangesAsync();
        }
    }
}

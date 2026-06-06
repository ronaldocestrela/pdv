using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Pdv.Modules.Sales.Application.Abstractions;

namespace Pdv.Tests.Integration;

public sealed class SalesIntegrationTests
{
    [Fact]
    public async Task Create_sale_requires_authentication()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/sales",
            new
            {
                items = new[] { new { productVariationId = 1, quantity = 1 } },
                paymentMethod = "cash",
            },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_sale_returns_bad_request_when_insufficient_stock()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var variationId = await IntegrationApiHelper.CreateProductAndVariationAsync(client, stockQuantity: 2, unitPrice: 10m);
        await IntegrationApiHelper.SyncVariationAsync(factory, variationId);

        var sale = await client.PostAsJsonAsync(
            "api/sales",
            new
            {
                items = new[] { new { productVariationId = variationId, quantity = 10 } },
                paymentMethod = "pix",
            },
            WebJson.Options);

        sale.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_sale_succeeds_and_lists_in_recent_sales()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var variationId = await IntegrationApiHelper.CreateProductAndVariationAsync(client, stockQuantity: 15, unitPrice: 11m);
        await IntegrationApiHelper.SyncVariationAsync(factory, variationId);

        var saleRes = await client.PostAsJsonAsync(
            "api/sales",
            new
            {
                items = new[] { new { productVariationId = variationId, quantity = 3 } },
                paymentMethod = "card",
            },
            WebJson.Options);

        saleRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await saleRes.Content.ReadFromJsonAsync<SaleCreatedResponse>(WebJson.Options);
        body.Should().NotBeNull();
        body!.TotalAmount.Should().Be(33m);

        var listRes = await client.GetAsync("api/sales?take=20");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listRes.Content.ReadFromJsonAsync<List<SaleListItemDto>>(WebJson.Options);
        list.Should().NotBeNull();
        list!.Should().Contain(s => s.Id == body.SaleId && s.TotalAmount == 33m && s.ItemCount == 1);
    }

    private sealed record SaleCreatedResponse(Guid SaleId, decimal TotalAmount);
}

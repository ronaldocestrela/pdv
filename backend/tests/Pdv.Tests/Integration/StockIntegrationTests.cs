using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Pdv.Application.Abstractions;

namespace Pdv.Tests.Integration;

public sealed class StockIntegrationTests
{
    [Fact]
    public async Task Stock_adjust_requires_authentication()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "api/stock/adjust",
            new { productVariationId = 1, quantity = 5, reason = (string?)null },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Stock_movements_requires_authentication()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("api/stock/movements?take=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Stock_adjust_creates_movement_visible_in_history()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var variationId = await IntegrationApiHelper.CreateProductAndVariationAsync(client, stockQuantity: 5);

        var adjust = await client.PostAsJsonAsync(
            "api/stock/adjust",
            new { productVariationId = variationId, quantity = 3, reason = "Entrada teste integração" },
            WebJson.Options);
        adjust.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var movementsRes = await client.GetAsync($"api/stock/movements?variationId={variationId}&take=50");
        movementsRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var movements = await movementsRes.Content.ReadFromJsonAsync<List<StockMovementListItemDto>>(WebJson.Options);
        movements.Should().NotBeNull();
        movements.Should().Contain(m =>
            m.ProductVariationId == variationId
            && m.Type.Equals("IN", StringComparison.OrdinalIgnoreCase)
            && m.Quantity == 3);
    }
}

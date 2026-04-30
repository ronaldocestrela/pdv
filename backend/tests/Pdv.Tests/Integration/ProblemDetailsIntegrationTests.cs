using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Pdv.Tests.Integration;

public sealed class ProblemDetailsIntegrationTests
{
    [Fact]
    public async Task Insufficient_stock_sale_returns_problem_details_json()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var variationId = await IntegrationApiHelper.CreateProductAndVariationAsync(client, stockQuantity: 2, unitPrice: 10m);

        var response = await client.PostAsJsonAsync(
            "api/sales",
            new
            {
                items = new[] { new { productVariationId = variationId, quantity = 10 } },
                paymentMethod = "pix",
            },
            WebJson.Options);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        await using var stream = await response.Content.ReadAsStreamAsync();
        var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        root.GetProperty("title").GetString().Should().Be("Validation failed");
        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("code").GetString().Should().Be("validation");
        root.TryGetProperty("correlationId", out var corr).Should().BeTrue();
        corr.GetString().Should().NotBeNullOrWhiteSpace();

        root.GetProperty("errors").ValueKind.Should().Be(JsonValueKind.Array);
        root.GetProperty("errors").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Authenticated_request_echoes_X_Correlation_Id()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var response = await client.GetAsync("api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        response.Headers.GetValues("X-Correlation-Id").First().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Request_with_X_Correlation_Id_header_echoes_same_value()
    {
        await using var factory = new PdvWebApplicationFactory();
        var client = factory.CreateClient();

        client.DefaultRequestHeaders.Add("X-Correlation-Id", "test-correlation-123");

        await IntegrationApiHelper.LoginAsAdminAsync(client);
        var response = await client.GetAsync("api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Correlation-Id").Single().Should().Be("test-correlation-123");
    }
}

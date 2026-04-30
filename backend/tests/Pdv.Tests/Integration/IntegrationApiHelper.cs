using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Pdv.Application.Auth;

namespace Pdv.Tests.Integration;

internal static class IntegrationApiHelper
{
    internal sealed record IdDto(int Id);

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

    public static async Task<int> CreateProductAndVariationAsync(
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
}

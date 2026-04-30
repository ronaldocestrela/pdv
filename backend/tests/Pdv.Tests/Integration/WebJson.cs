using System.Text.Json.Serialization;
using System.Text.Json;

namespace Pdv.Tests.Integration;

internal static class WebJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };
}

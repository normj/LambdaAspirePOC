using System.Text.Json.Serialization;

namespace Aspire.Hosting.AWS.Lambda;

internal class RouteConfig
{
    public RouteConfig(string lambdaResourceName, string endpoint, Method httpMethod, string path)
    {
        LambdaResourceName = lambdaResourceName;
        Endpoint = endpoint;
        HttpMethod = httpMethod;
        Path = path;
    }

    public string LambdaResourceName { get; init; }

    public string Endpoint { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Method HttpMethod { get; init; }

    public string Path { get; init; }
}

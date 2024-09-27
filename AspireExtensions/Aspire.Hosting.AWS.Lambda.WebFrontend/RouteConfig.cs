using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aspire.Hosting.AWS.Lambda.WebFrontend;

internal class RouteConfig
{
    public RouteConfig(string lambdaResourceName, string endpoint, Method httpMethod, string path)
    {
        LambdaResourceName = lambdaResourceName;
        Endpoint = endpoint;
        HttpMethod = httpMethod;
        Path = path;

        var sbEmulatorEndpoint = new StringBuilder();
        sbEmulatorEndpoint.Append(endpoint);
        if (!endpoint.EndsWith("/"))
        {
            sbEmulatorEndpoint.Append("/");
        }
        sbEmulatorEndpoint.Append(lambdaResourceName);
        sbEmulatorEndpoint.Append("/");

        HttpClient = new HttpClient
        {
            BaseAddress = new Uri(sbEmulatorEndpoint.ToString())
        };
    }

    public string LambdaResourceName { get; init; }

    public string Endpoint { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Method HttpMethod { get; init; }

    public string Path { get; init; }

    public HttpClient HttpClient { get; }

    internal static IList<RouteConfig> LoadEnvironmentRouteConfigs(ILogger logger)
    {
        const string ENVIR_PREFIX = "APIGATEWAY_EMULATOR_ROUTE_CONFIG_";

        var options = new JsonSerializerOptions
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
        };

        var routes = new List<RouteConfig>();
        foreach (string key in Environment.GetEnvironmentVariables().Keys)
        {
            if (!key.StartsWith(ENVIR_PREFIX))
                continue;

            var configJson = Environment.GetEnvironmentVariable(key);
            logger.LogDebug("Found route config environment variable {name} with value {value}", key, configJson);


            var config = JsonSerializer.Deserialize<RouteConfig>(configJson!, options);
            if (config == null)
            {
                logger.LogWarning("Failed to deserialize route config {config}", configJson);
                continue;
            }

            logger.LogInformation("Added log config for {functionName} for method {method} and path {path}", config.LambdaResourceName, config.HttpMethod, config.Path);
            routes.Add(config);
        }

        return routes;
    }

    internal static RouteConfig ChooseRouteConfig(IList<RouteConfig> routes, string method, string path)
    {
        // TODO: Evaluate all of the route configs to determine the best match route config. Don't forget
        // to handle {proxy+} wild cards.
        return routes.First()!;
    }
}

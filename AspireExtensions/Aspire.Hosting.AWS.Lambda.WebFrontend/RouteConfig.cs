using System.Diagnostics.CodeAnalysis;
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

    internal static ChooseRouteConfigResult? ChooseRouteConfig(IList<RouteConfig> routes, string method, string path)
    {
        var methodEnum = Enum.Parse<Method>(method, true);

        foreach(var route in routes)
        {
            // TODO: need to handle wild card proxy tokens in the path

            if (route.HttpMethod != Method.Any && route.HttpMethod != methodEnum)
            {
                continue;
            }

            var pathTokens = path.Split('/');
            var routeTokens = route.Path.Split('/');

            if (TryRouteMatch(routeTokens, pathTokens, out var pathParameters))
            {
                return new ChooseRouteConfigResult(route, pathParameters);
            }
        }

        return null;
    }

    internal class ChooseRouteConfigResult
    {
        internal ChooseRouteConfigResult(RouteConfig routeConfig, Dictionary<string, string> pathParameters)
        {
            RouteConfig = routeConfig;
            PathVariables = pathParameters;
        }

        public RouteConfig RouteConfig { get; }

        public Dictionary<string, string> PathVariables { get; }
    }


    internal static bool TryRouteMatch(string[] routeTokens, string[] pathTokens, [NotNullWhen(true)] out Dictionary<string, string>? pathParameters)
    {
        pathParameters = new Dictionary<string, string>();
        for (int i = 0; i < routeTokens.Length; i++)
        {
            if (pathTokens.Length <= i)
            {
                pathParameters = null;
                return false;
            }
            else if (TryPathVariable(routeTokens[i], out var pathVariable))
            {
                pathParameters[pathVariable] = pathTokens[i];
            }
            else if (!string.Equals(routeTokens[i], pathTokens[i], StringComparison.OrdinalIgnoreCase))
            {
                pathParameters = null;
                return false;
            }
        }

        return true;
    }

    internal static bool TryPathVariable(string routeToken, [NotNullWhen(true)] out string? pathParameters)
    {
        pathParameters = null;

        routeToken = routeToken.Trim();
        if (routeToken.Length > 2 && routeToken[0] == '{' && routeToken[routeToken.Length - 1] == '}')
        {
            pathParameters = routeToken.Substring(1, routeToken.Length - 2);
            return true;
        }
        return false;
    }
}

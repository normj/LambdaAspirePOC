using Amazon.Lambda.APIGatewayEvents;
using Aspire.Hosting.AWS.Lambda.WebFrontend;
using System.Text;
using System.Text.Unicode;

// TODO: Parse APIGateway type from command line arguments

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var routeConfigs = RouteConfig.LoadEnvironmentRouteConfigs(logger);

app.Use(async (HttpContext ctx, Func<Task> _) =>
{
    var routeConfigResult = RouteConfig.ChooseRouteConfig(routeConfigs, ctx.Request.Method, ctx.Request.Path);

    if (routeConfigResult == null)
    {
        const string notFoundResponse = "{\"message\":\"Not Found\"}";
        var responseBytes = Encoding.UTF8.GetBytes(notFoundResponse);
        ctx.Response.StatusCode = 404;
        ctx.Response.ContentType = "application/json";
        ctx.Response.ContentLength = responseBytes.Length;
        await ctx.Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);
        return;
    }

    // TODO: Handle APIGateway Type. Currently always using HttpApi, needs to handle RestApi and RestApiV2.
    var lambdaRequest = await Translators.TranslateToRequestAsync(ctx.Request, routeConfigResult.PathVariables);

    using var response = await routeConfigResult.RouteConfig.HttpClient.PostAsJsonAsync("runtime/test-event-sync", lambdaRequest);

    if (!response.IsSuccessStatusCode)
    {
        ctx.Response.StatusCode = 500;

        var errorResponse = await response.Content.ReadAsByteArrayAsync();
        await ctx.Response.Body.WriteAsync(errorResponse, 0, errorResponse.Length);
        return;
    }

    var apiGatewayResponse = System.Text.Json.JsonSerializer.Deserialize<APIGatewayHttpApiV2ProxyResponse>(await response.Content.ReadAsStringAsync());
    if (apiGatewayResponse == null)
    {
        ctx.Response.StatusCode = 500;
        return;
    }

    ctx.Response.StatusCode = apiGatewayResponse.StatusCode;
    foreach(var kvp in apiGatewayResponse.Headers)
    {
        ctx.Response.Headers[kvp.Key] = kvp.Value;
    }

    if (apiGatewayResponse.Body != null)
    {
        Byte[] responseBody;
        if (apiGatewayResponse.IsBase64Encoded)
        {
            responseBody = Convert.FromBase64String(apiGatewayResponse.Body);
        }
        else
        {
            responseBody = System.Text.Encoding.UTF8.GetBytes(apiGatewayResponse.Body);
        }

        ctx.Response.ContentLength = responseBody.Length;
        await ctx.Response.Body.WriteAsync(responseBody, 0, responseBody.Length);
    }
});

app.Run();


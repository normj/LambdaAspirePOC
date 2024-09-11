
using Amazon.Lambda.APIGatewayEvents;
using Aspire.Hosting.AWS.Lambda.WebFrontend;
using System.Text.Unicode;

var lambdaResourceName = Environment.GetEnvironmentVariable("LAMBDA_FUNCTION_RESOURCE_NAME");
if (lambdaResourceName == null)
{
    throw new ArgumentNullException("Missing LAMBDA_FUNCTION_RESOURCE_NAME environment variable");
}

var emulatorEnvName = $"services__LambdaEmulator-{lambdaResourceName}__http__0";
var lambdaEmulatorUrl = Environment.GetEnvironmentVariable(emulatorEnvName);
if (lambdaEmulatorUrl == null)
{
    throw new ArgumentNullException($"Missing {emulatorEnvName} environment variable");
}

Console.WriteLine("Lambda emulator endpoint: " + lambdaEmulatorUrl);

var emulatorHttpClient = new HttpClient();
emulatorHttpClient.BaseAddress = new Uri(lambdaEmulatorUrl);

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseHttpsRedirection();


app.Use(async (HttpContext ctx, Func<Task> _) =>
{
    var lambdaRequest = await Translators.TranslateToRequestAsync(ctx.Request);

    var response = await emulatorHttpClient.PostAsJsonAsync("/runtime/test-event-sync", lambdaRequest);

    var apiGatewayResponse = System.Text.Json.JsonSerializer.Deserialize<APIGatewayHttpApiV2ProxyResponse>(await response.Content.ReadAsStringAsync());

    ctx.Response.StatusCode = apiGatewayResponse.StatusCode;
    foreach(var kvp in apiGatewayResponse.Headers)
    {
        ctx.Response.Headers[kvp.Key] = kvp.Value;
    }

    var bytes = System.Text.Encoding.UTF8.GetBytes(apiGatewayResponse.Body);
    ctx.Response.ContentLength = bytes.Length;
    await ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length);
});

app.Run();


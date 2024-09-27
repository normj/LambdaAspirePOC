using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Text.Unicode;

namespace LambdaAspirePOC.E2E.Tests;

[TestClass]
public class E2ETests
{
    [TestMethod]
    public async Task InvokeThroughAPIGatewayEmulator()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LambdaAspirePOC_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
            .WaitForResourceAsync("APIGatewayEmulator", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        using var apigatewayEmulatorHttpClient = app.CreateHttpClient("APIGatewayEmulator");

        var result = await apigatewayEmulatorHttpClient.GetStringAsync("/add/4/5");
        Assert.AreEqual("9", result);
    }

    [TestMethod]
    public async Task InvokeThroughSDK()
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LambdaAspirePOC_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await resourceNotificationService
            .WaitForResourceAsync("Lambda-ServiceEmulator", KnownResourceStates.Running)
            .WaitAsync(TimeSpan.FromSeconds(30));

        using var lambdaEndpointClient = app.CreateHttpClient("Lambda-ServiceEmulator");

        var lambdaConfig = new AmazonLambdaConfig
        {
            ServiceURL = lambdaEndpointClient.BaseAddress!.ToString()
        };
        var lambdaClient = new AmazonLambdaClient(lambdaConfig);

        var invokeRequest = new InvokeRequest
        {
            FunctionName = "ClassLibraryLambdaFunction",
            Payload = "\"hello world\"",
            InvocationType = InvocationType.RequestResponse
        };
        var response = await lambdaClient.InvokeAsync(invokeRequest);
        var lambdaResponse = new StreamReader(response.Payload).ReadToEnd();
        Assert.AreEqual("\"HELLO WORLD\"", lambdaResponse);
    }
}

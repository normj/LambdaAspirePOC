using Aspire.Hosting.AWS.Lambda;
using Aspire.Hosting.Lifecycle;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aspire.Hosting;

public static class LambdaExtensions
{
    public static IResourceBuilder<ProjectResource> AddLambdaFunction<TLambdaProject>(this IDistributedApplicationBuilder builder, string name, string handler) where TLambdaProject : IProjectMetadata, new()
    {
        var metadata = new TLambdaProject();

        var serviceEmulator = AddOrGetLambdaServiceEmulatorResource(builder);

        IResourceBuilder<ProjectResource> resource;
        if (handler.Contains("::"))
        {
            var method = handler.Split("::").Last();
            resource = builder.AddProject<TLambdaProject>(name, "LambdaRuntimeClient_" + method);
        }
        else
        {
            var project = new LambdaProjectResource(name);
            resource = builder.AddResource(project)
                            .WithAnnotation(new TLambdaProject());
        }

        resource.WithEnvironment(context =>
                    {
                        var serviceEmulatorEndpoint = serviceEmulator.GetEndpoint("http");

                        // Add the Lambda function resource on the path so the emulator can distingish request
                        // for each Lambda function.
                        var apiPath = $"{serviceEmulatorEndpoint.Host}:{serviceEmulatorEndpoint.Port}/{name}";
                        context.EnvironmentVariables["AWS_LAMBDA_RUNTIME_API"] = apiPath;
                    });


        resource.WithAnnotation(new LambdaFunctionAnnotation());

        return resource;
    }

    public static IResourceBuilder<APIGatewayApiResource> AddAPIGatewayEmulator(this IDistributedApplicationBuilder builder, string name, APIGatewayType apiGatewayType)
    {
        var apiGatewayEmulator = builder.AddResource(new APIGatewayApiResource(name)).ExcludeFromManifest();
        apiGatewayEmulator.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

        apiGatewayEmulator.WithArgs(context =>
        {
            context.Args.Add("exec");
            context.Args.Add("bin\\Debug\\net8.0\\Aspire.Hosting.AWS.Lambda.WebFrontend.dll");
            context.Args.Add("--apigatewaytype");
            context.Args.Add(apiGatewayType.ToString());
        });

        var annotation = new EndpointAnnotation(
            protocol: ProtocolType.Tcp,
            uriScheme: "http");

        apiGatewayEmulator.WithAnnotation(annotation);
        var endpointReference = new EndpointReference(apiGatewayEmulator.Resource, annotation);

        apiGatewayEmulator.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            var aspnetCoreUrls = new ReferenceExpressionBuilder();
            aspnetCoreUrls.Append($"{endpointReference.Property(EndpointProperty.Scheme)}://localhost:{endpointReference.Property(EndpointProperty.TargetPort)}");
            context.EnvironmentVariables["ASPNETCORE_URLS"] = aspnetCoreUrls.Build();
        }));

        apiGatewayEmulator.WithAnnotation(new APIGatewayEmulatorAnnotation(apiGatewayType));

        return apiGatewayEmulator;
    }

    public static IResourceBuilder<APIGatewayApiResource> WithReference(this IResourceBuilder<APIGatewayApiResource> builder, IResourceBuilder<LambdaProjectResource> lambda, Method httpMethod, string path)
    {
        LambdaEmulatorAnnotation? lambdaEmulatorAnnotation = null;
        if (builder.ApplicationBuilder.Resources.FirstOrDefault(x => x.TryGetLastAnnotation<LambdaEmulatorAnnotation>(out lambdaEmulatorAnnotation)) == null ||
            lambdaEmulatorAnnotation == null)
        {
            return builder;
        }

        builder.WithReference(lambdaEmulatorAnnotation.Endpoint);

        builder.WithEnvironment(context =>
        {
            var envName = "APIGATEWAY_EMULATOR_ROUTE_CONFIG_" + lambda.Resource.Name;
            var config = new RouteConfig(lambda.Resource.Name, lambdaEmulatorAnnotation.Endpoint.Url, httpMethod, path);
            var configJson = JsonSerializer.Serialize(config);
            context.EnvironmentVariables[envName] = configJson;
        });

        return builder;
    }

    private static ExecutableResource AddOrGetLambdaServiceEmulatorResource(IDistributedApplicationBuilder builder)
    {
        var serviceEmulator = builder.Resources.FirstOrDefault(x => x.TryGetAnnotationsOfType<LambdaEmulatorAnnotation>(out _)) as ExecutableResource;
        if (serviceEmulator == null)
        {
            var serviceEmulatorBuilder = builder.AddExecutable($"Lambda-ServiceEmulator",
                                                    "dotnet",
                                                    // TODO Detect the working directory based on relative location from this Assembly.
                                                    "C:\\gitrepos\\LambdaAspirePOC\\AspireExtensions\\Aspire.Hosting.AWS.LambdaServiceEmulator",
                                                    "exec",
                                                    "bin\\Debug\\net8.0\\Aspire.Hosting.AWS.LambdaServiceEmulator.dll")
                                    .ExcludeFromManifest();

            
            serviceEmulatorBuilder.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

            var annotation = new EndpointAnnotation(
                protocol: ProtocolType.Tcp,
                uriScheme: "http");

            serviceEmulatorBuilder.WithAnnotation(annotation);
            var endpointReference = new EndpointReference(serviceEmulatorBuilder.Resource, annotation);

            serviceEmulatorBuilder.WithAnnotation(new LambdaEmulatorAnnotation(endpointReference));

            serviceEmulatorBuilder.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
            {
                var aspnetCoreUrls = new ReferenceExpressionBuilder();
                aspnetCoreUrls.Append($"{endpointReference.Property(EndpointProperty.Scheme)}://localhost:{endpointReference.Property(EndpointProperty.TargetPort)}");
                context.EnvironmentVariables["ASPNETCORE_URLS"] = aspnetCoreUrls.Build();
            }));

            serviceEmulator = serviceEmulatorBuilder.Resource;
        }

        return serviceEmulator;
    }
}

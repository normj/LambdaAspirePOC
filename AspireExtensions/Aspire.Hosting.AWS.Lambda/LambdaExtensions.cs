using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.AWS.Lambda;
using System.Net.Sockets;

namespace Aspire.Hosting;

public static class LambdaExtensions
{
    public static IResourceBuilder<ProjectResource> AddLambdaFunction<TLambdaProject>(this IDistributedApplicationBuilder builder, string name) where TLambdaProject : ILambdaFunctionMetadata, new()
    {
        var metadata = new TLambdaProject();
        var emulator = builder.AddExecutable($"LambdaEmulator-{name}",
                                                "dotnet",
                                                // TODO Detect the working directory based on relative location from this Assembly.
                                                "C:\\gitrepos\\LambdaAspirePOC\\AspireExtensions\\Aspire.Hosting.AWS.LambdaEmulator",
                                                "exec",
                                                "bin\\Debug\\net8.0\\Aspire.Hosting.AWS.LambdaEmulator.dll")
                                .ExcludeFromManifest();

        emulator.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
        emulator.WithEnvironment("LAMBDA_FUNCTION_RESOURCE_NAME", name);

        var annotation = new EndpointAnnotation(
            protocol: ProtocolType.Tcp,
            uriScheme: "http");

        emulator.WithAnnotation(annotation);
        var endpointReference = new EndpointReference(emulator.Resource, annotation);

        emulator.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            var aspnetCoreUrls = new ReferenceExpressionBuilder();
            aspnetCoreUrls.Append($"{endpointReference.Property(EndpointProperty.Scheme)}://localhost:{endpointReference.Property(EndpointProperty.TargetPort)}");
            context.EnvironmentVariables["ASPNETCORE_URLS"] = aspnetCoreUrls.Build();
        }));

        //var project = new LambdaProjectResource(name);
        //var lambdaResource = builder.AddResource<TLambdaProject>(project);


        IResourceBuilder<ProjectResource> resource;
        if (metadata.IsClassLibrary)
        {
            resource = builder.AddProject<TLambdaProject>(name, "LambdaRuntimeClient");
        }
        else
        {
            resource = builder.AddProject<TLambdaProject>(name);
        }

        resource.WithEnvironment(context =>
                    {
                        var emulatorEndpoint = emulator.GetEndpoint("http");
                        context.EnvironmentVariables["AWS_LAMBDA_RUNTIME_API"] = $"{emulatorEndpoint.Host}:{emulatorEndpoint.Port}";
                    });


        return resource;
    }
}

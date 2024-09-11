using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.AWS.Lambda;
using System.Net.Sockets;
using System.Xml.Linq;

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
            var project = new LambdaProjectResource(name);
            resource = builder.AddResource(project)
                            .WithAnnotation(new TLambdaProject());
        }

        resource.WithEnvironment(context =>
                    {
                        var emulatorEndpoint = emulator.GetEndpoint("http");
                        context.EnvironmentVariables["AWS_LAMBDA_RUNTIME_API"] = $"{emulatorEndpoint.Host}:{emulatorEndpoint.Port}";
                    });


        resource.WithAnnotation(new LambdaEmulatorAnnotation(endpointReference, emulator));


        return resource;
    }

    public static IResourceBuilder<LambdaProjectResource> WithWebEventSource(this IResourceBuilder<LambdaProjectResource> resourceBuilder, WebEventSourceType frontendType)
    {
        if (!resourceBuilder.Resource.TryGetLastAnnotation<LambdaEmulatorAnnotation>(out var lambdaEmulatorAnnotation))
        {
            return resourceBuilder;
        }

        resourceBuilder.WithAnnotation(new WebFrontendAnnotation(frontendType));

        var webEventSource = resourceBuilder.ApplicationBuilder.AddExecutable($"LambdaWebFrontend-{resourceBuilder.Resource.Name}",
                                                "dotnet",
                                                // TODO Detect the working directory based on relative location from this Assembly.
                                                "C:\\gitrepos\\LambdaAspirePOC\\AspireExtensions\\Aspire.Hosting.AWS.Lambda.WebFrontend",
                                                "exec",
                                                "bin\\Debug\\net8.0\\Aspire.Hosting.AWS.Lambda.WebFrontend.dll")
                                .ExcludeFromManifest();

        webEventSource.WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");
        webEventSource.WithEnvironment("LAMBDA_FUNCTION_RESOURCE_NAME", resourceBuilder.Resource.Name);

        var annotation = new EndpointAnnotation(
            protocol: ProtocolType.Tcp,
            uriScheme: "http");

        webEventSource.WithAnnotation(annotation);
        var endpointReference = new EndpointReference(webEventSource.Resource, annotation);


        webEventSource.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        {
            var aspnetCoreUrls = new ReferenceExpressionBuilder();
            aspnetCoreUrls.Append($"{endpointReference.Property(EndpointProperty.Scheme)}://localhost:{endpointReference.Property(EndpointProperty.TargetPort)}");
            context.EnvironmentVariables["ASPNETCORE_URLS"] = aspnetCoreUrls.Build();
        }));

        webEventSource.WithReference(lambdaEmulatorAnnotation.Endpoint);


        //lambdaEmulatorAnnotation.ExecutableBuilder.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        //{
        //    var emulatorUrl = new ReferenceExpressionBuilder();
        //    emulatorUrl.Append($"{lambdaEmulatorAnnotation.Endpoint.Property(EndpointProperty.Scheme)}://localhost:{lambdaEmulatorAnnotation.Endpoint.Property(EndpointProperty.TargetPort)}");
        //    webEventSource.WithEnvironment("LAMBDA_EMULATOR_URL", emulatorUrl.Build());
        //}));

        //webEventSource.WithAnnotation(new EnvironmentCallbackAnnotation(context =>
        //{
        //    var emulatorUrl = new ReferenceExpressionBuilder();
        //    emulatorUrl.Append($"{lambdaEmulatorAnnotation.Endpoint.Property(EndpointProperty.Scheme)}://localhost:{lambdaEmulatorAnnotation.Endpoint.Property(EndpointProperty.TargetPort)}");
        //    context.EnvironmentVariables["LAMBDA_EMULATOR_URL"] = emulatorUrl.Build();
        //}));

        return resourceBuilder;
    }
}

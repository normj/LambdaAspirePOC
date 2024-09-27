namespace Aspire.Hosting.AWS.Lambda;

internal class LambdaEmulatorAnnotation(EndpointReference endpoint) : IResourceAnnotation
{
    public EndpointReference Endpoint { get; init; } = endpoint;
}

namespace Aspire.Hosting.AWS.Lambda;

internal class LambdaFunctionAnnotation(string handler) : IResourceAnnotation
{
    public string Handler { get; } = handler;
}

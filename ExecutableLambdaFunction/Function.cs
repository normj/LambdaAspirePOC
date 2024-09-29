using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;

// The function handler that will be called for each Lambda event
var ToUpper = (string input, ILambdaContext context) =>
{
    Console.WriteLine("Input: " + input);
    return input.ToUpper();
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(ToUpper, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();
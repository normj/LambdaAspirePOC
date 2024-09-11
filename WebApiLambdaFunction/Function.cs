using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.APIGatewayEvents;

// The function handler that will be called for each Lambda event
var handler = (APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context) =>
{
    APIGatewayHttpApiV2ProxyResponse response;
    try
    {
        var result = PerformOperation(request.RawPath);

        response = new APIGatewayHttpApiV2ProxyResponse
        {
            StatusCode = 200,
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "text/plain" }
            }
        };

        if (result == null)
        {
            response.Body = "Welcome to the Lambda calculator. Try resource paths like \"/add/3/4\" to perform a calulation";
        }
        else
        {
            response.Body = result.ToString();
        }
    }
    catch(Exception ex)
    {
        response = new APIGatewayHttpApiV2ProxyResponse
        {
            Body = ex.Message,
            StatusCode = 304,
            Headers = new Dictionary<string, string>
            {
                {"Content-Type", "text/plain" }
            }
        };
    }

    Console.WriteLine($"Status Code: {response.StatusCode}, Body {response.Body}");
    return response;
};

int? PerformOperation(string path)
{
    if (path.StartsWith("/"))
        path = path.Substring(1);

    var tokens = path.Split('/');
    if (tokens.Length != 3)
        return null;

    var op1 = int.Parse(tokens[1]);
    var op2 = int.Parse(tokens[2]);

    switch(tokens[0])
    {
        case "add":
            return op1 + op2;
        case "minus":
            return op1 - op2;
        case "times":
            return op1 * op2;
        case "div":
            return op1 / op2;
        default:
            throw new Exception("Unknown operand: " + tokens[0]);
    }
}

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();
using Amazon.Lambda.APIGatewayEvents;

namespace Aspire.Hosting.AWS.Lambda.WebFrontend;

public static class Translators
{
    public static async Task<APIGatewayHttpApiV2ProxyRequest> TranslateToRequestAsync(HttpRequest httpRequest)
    {
        // TODO: Handle all of the mapping from ASP.NET Core request to API Gateway request. That includes
        // grabbing resource path parameters.

        var lambdaRequest = new APIGatewayHttpApiV2ProxyRequest()
        {
            RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
            {
                Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
                {
                    Method = httpRequest.Method,
                    Path = httpRequest.Path,
                    Protocol = httpRequest.Protocol,
                }
            }
        };        
        
        lambdaRequest.RawPath = httpRequest.Path;

        lambdaRequest.Headers = new Dictionary<string, string>();
        foreach(var kvp in httpRequest.Headers)
        {
            lambdaRequest.Headers[kvp.Key] = kvp.Value;
        }

        lambdaRequest.Body = await (new StreamReader(httpRequest.Body)).ReadToEndAsync();

        return lambdaRequest;
    }
}

using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

IConfiguration configuration = new ConfigurationBuilder()
                                        .AddEnvironmentVariables()
                                        .Build();
var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("cache")!);
var redisDatabase = redis.GetDatabase();



// The function handler that will be called for each Lambda event
var ToUpper = (string input, ILambdaContext context) =>
{
    context.Logger.LogInformation("Input: " + input);
    string? upper = null;
    var cacheResult = redisDatabase.StringGet(new RedisKey(input));
    if (!cacheResult.HasValue)
    {
        context.Logger.LogInformation("Cache miss");
        upper = input.ToUpper();
        redisDatabase.StringSet(new RedisKey(input), new RedisValue(upper));
    }
    else
    {
        upper = cacheResult.ToString();
        context.Logger.LogInformation("Cache hit");
    }

    context.Logger.LogInformation("Upper: " + upper);
    return upper;
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(ToUpper, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();
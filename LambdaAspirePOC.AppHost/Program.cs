using Amazon;
using Aspire.Hosting.AWS.Lambda;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var awsConfig = builder.AddAWSSDKConfig()
                        .WithProfile("default")
                        .WithRegion(RegionEndpoint.USWest2);

builder.AddAWSLambdaFunction<Projects.ExecutableLambdaFunction>("ToUpperLambdaFunction", handler: "ExecutableLambdaFunction")
        .WithReference(cache)
        .WithReference(awsConfig);

builder.AddAWSLambdaFunction<Projects.ClassLibraryLambdaFunction>("ToLowerLambdaFunction", handler: "ClassLibraryLambdaFunction::ClassLibraryLambdaFunction.Function::FunctionHandler")
        .WithReference(awsConfig);

#region Part 2
// TODO: Get rid of the casts that are due to ClassLibrary and Executable projects returning different types.
//var rootWebFunction = builder.AddAWSLambdaFunction<Projects.WebApiLambdaFunction>("RootLambdaFunction", handler: "WebApiLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
//var addFunction = builder.AddAWSLambdaFunction<Projects.WebAddLambdaFunction>("AddLambdaFunction", handler: "WebAddLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
//var minusFunction = builder.AddAWSLambdaFunction<Projects.WebMinusLambdaFunction>("MinusLambdaFunction", handler: "WebMinusLambdaFunction") as IResourceBuilder<LambdaProjectResource>;


//builder.AddAPIGatewayEmulator("APIGatewayEmulator", APIGatewayType.HttpApi)
//        .WithReference(rootWebFunction, Method.Get, "/")
//        .WithReference(addFunction, Method.Get, "/add/{x}/{y}")
//        .WithReference(minusFunction, Method.Get, "/minus/{x}/{y}");
#endregion


builder.Build().Run();
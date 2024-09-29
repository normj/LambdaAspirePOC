using Amazon;
using Aspire.Hosting.AWS.Lambda;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
                        .WithProfile("default")
                        .WithRegion(RegionEndpoint.USWest2);


builder.AddLambdaFunction<Projects.ExecutableLambdaFunction>("ToUpperLambdaFunction", "ExecutableLambdaFunction")
        .WithReference(awsConfig);

builder.AddLambdaFunction<Projects.ClassLibraryLambdaFunction>("ToLowerLambdaFunction", "ClassLibraryLambdaFunction::ClassLibraryLambdaFunction.Function::FunctionHandler")
        .WithReference(awsConfig);

// TODO: Get rid of the casts that are due to ClassLibrary and Executable projects returning different types.
var rootWebFunction = builder.AddLambdaFunction<Projects.WebApiLambdaFunction>("RootLambdaFunction", "WebApiLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
var addFunction = builder.AddLambdaFunction<Projects.WebAddLambdaFunction>("AddLambdaFunction", "WebAddLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
var minusFunction = builder.AddLambdaFunction<Projects.WebMinusLambdaFunction>("MinusLambdaFunction", "WebMinusLambdaFunction") as IResourceBuilder<LambdaProjectResource>;


builder.AddAPIGatewayEmulator("APIGatewayEmulator", APIGatewayType.HttpApi)
        .WithReference(rootWebFunction, Method.Get, "/")
        .WithReference(addFunction, Method.Get, "/add/{x}/{y}")
        .WithReference(minusFunction, Method.Get, "/minus/{x}/{y}");


builder.Build().Run();

using Amazon;
using Aspire.Hosting.AWS.Lambda;
using LambdaAspirePOC.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var awsConfig = builder.AddAWSSDKConfig()
                        .WithProfile("default")
                        .WithRegion(RegionEndpoint.USWest2);


builder.AddLambdaFunction<LambdaFunctions.ExecutableLambdaFunction>("ExecutableLambdaFunction")
        .WithReference(awsConfig);

builder.AddLambdaFunction<LambdaFunctions.ClassLibraryLambdaFunction>("ClassLibraryLambdaFunction")
        .WithReference(awsConfig);

// TODO: Get rid of the casts that are due to ClassLibrary and Executable projects returning different types.
var rootWebFunction = builder.AddLambdaFunction<LambdaFunctions.WebApiLambdaFunction>("RootLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
var addFunction = builder.AddLambdaFunction<LambdaFunctions.WebAddLambdaFunction>("AddLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
var minusFunction = builder.AddLambdaFunction<LambdaFunctions.WebMinusLambdaFunction>("MinusLambdaFunction") as IResourceBuilder<LambdaProjectResource>;


builder.AddAPIGatewayEmulator("APIGatewayEmulator", APIGatewayType.HttpApi)
        .WithReference(rootWebFunction, Method.Get, "/")
        .WithReference(addFunction, Method.Get, "/add/{x}/{y}")
        .WithReference(minusFunction, Method.Get, "/minus/{x}/{y}");


builder.Build().Run();

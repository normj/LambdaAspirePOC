

using LambdaAspirePOC.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddLambdaFunction<LambdaFunctions.ExecutableLambdaFunction>("ExecutableLambdaFunction");
builder.AddLambdaFunction<LambdaFunctions.ClassLibraryLambdaFunction>("ClassLibraryLambdaFunction");

builder.Build().Run();

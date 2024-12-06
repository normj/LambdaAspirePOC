using Amazon;
using Aspire.Hosting.AWS.Lambda;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddAWSLambdaFunction<Projects.ClassLibraryLambdaFunction>("ToLowerLambdaFunction", handler: "ClassLibraryLambdaFunction::ClassLibraryLambdaFunction.Function::FunctionHandler");

builder.Build().Run();
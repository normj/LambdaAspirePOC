

using Aspire.Hosting.AWS.Lambda;
using LambdaAspirePOC.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddLambdaFunction<LambdaFunctions.ExecutableLambdaFunction>("ExecutableLambdaFunction");
builder.AddLambdaFunction<LambdaFunctions.ClassLibraryLambdaFunction>("ClassLibraryLambdaFunction");

#region HTTP Lambda Function 
//var httpFunction = builder.AddLambdaFunction<LambdaFunctions.WebApiLambdaFunction>("WebApiLambdaFunction") as IResourceBuilder<LambdaProjectResource>;
//// TODO: This is cast to IResourceBuilder<LambdaProjectResource> is temporary till I can fix AddLambdaFunction to return that type.
//// Currently AddLambdaFunction only returns IResourceBuilder<LambdaProjectResource> for executable lambda projects.
//if (httpFunction == null)
//{
//    throw new Exception("Broke cast of LambdaProjectResource");
//}
//httpFunction.WithHttpEventSource(WebEventSourceType.HttpApi);
#endregion

builder.Build().Run();

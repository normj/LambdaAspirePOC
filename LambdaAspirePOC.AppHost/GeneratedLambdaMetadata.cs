using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aspire.Hosting.AWS.Lambda;

namespace LambdaAspirePOC.AppHost;

public class LambdaFunctions
{
    public class ExecutableLambdaFunction : LambdaFunctionMetadata
    {
        public ExecutableLambdaFunction() :
            base(@"C:\gitrepos\LambdaAspirePOC\ExecutableLambdaFunction\ExecutableLambdaFunction.csproj", "ExecutableLambdaFunction", false)
        {

        }
    }

    public class ClassLibraryLambdaFunction : LambdaFunctionMetadata
    {
        public ClassLibraryLambdaFunction() :
            base(@"C:\gitrepos\LambdaAspirePOC\ClassLibraryLambdaFunction\ClassLibraryLambdaFunction.csproj", "ClassLibraryLambdaFunction::ClassLibraryLambdaFunction.Function::FunctionHandler", true)
        {
            
        }
    }

    public class WebApiLambdaFunction : LambdaFunctionMetadata
    {
        public WebApiLambdaFunction() :
            base(@"C:\gitrepos\LambdaAspirePOC\WebApiLambdaFunction\WebApiLambdaFunction.csproj", "WebApiLambdaFunction", false)
        {

        }
    }
}

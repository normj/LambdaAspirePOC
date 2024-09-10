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
            base(@"C:\Users\normj\source\repos\LambdaAspirePOC\ExecutableLambdaFunction\ExecutableLambdaFunction.csproj", "ExecutableLambdaFunction", false)
        {

        }
    }

    public class ClassLibraryLambdaFunction : LambdaFunctionMetadata
    {
        public ClassLibraryLambdaFunction() :
            base(@"C:\Users\normj\source\repos\LambdaAspirePOC\ClassLibraryLambdaFunction\ClassLibraryLambdaFunction.csproj", "ClassLibraryLambdaFunction::ClassLibraryLambdaFunction.Function::FunctionHandler", true)
        {
            
        }
    }
}

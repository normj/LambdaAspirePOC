using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

public class APIGatewayApiResource(string name) : ExecutableResource(name, 
        "dotnet", 
        "C:\\gitrepos\\LambdaAspirePOC\\AspireExtensions\\Aspire.Hosting.AWS.Lambda.WebFrontend"
        )
{
}

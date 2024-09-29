using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

public class APIGatewayApiResource(string name, string emulatorProjectDirectory) : ExecutableResource(name, 
        "dotnet",
        emulatorProjectDirectory
        )
{
}

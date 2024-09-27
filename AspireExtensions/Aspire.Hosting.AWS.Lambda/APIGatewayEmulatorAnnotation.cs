using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

internal class APIGatewayEmulatorAnnotation(APIGatewayType apiGatewayType) : IResourceAnnotation
{
    public APIGatewayType ApiGatewayType { get; set; } = apiGatewayType;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

internal class LambdaEmulatorAnnotation : IResourceAnnotation
{
    public LambdaEmulatorAnnotation(EndpointReference endpoint, IResourceBuilder<ExecutableResource> executableBuilder)
    {
        Endpoint = endpoint;
        ExecutableBuilder = executableBuilder;
    }

    public EndpointReference Endpoint { get; init; }

    public IResourceBuilder<ExecutableResource> ExecutableBuilder { get; init; }
}

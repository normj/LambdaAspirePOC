using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

internal class HttpEventSourceAnnotation : IResourceAnnotation
{
    public HttpEventSourceAnnotation (HttpEventSourceType frontendType)
    {
        FrontendType = frontendType;
    }

    public HttpEventSourceType FrontendType { get; init; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

internal class WebFrontendAnnotation : IResourceAnnotation
{
    public WebFrontendAnnotation (WebEventSourceType frontendType)
    {
        FrontendType = frontendType;
    }

    public WebEventSourceType FrontendType { get; init; }
}

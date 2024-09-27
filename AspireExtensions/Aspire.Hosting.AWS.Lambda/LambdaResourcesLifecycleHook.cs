using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspire.Hosting.AWS.Lambda;

internal class LambdaResourcesLifecycleHook(ResourceNotificationService resourceNotificationService) : IDistributedApplicationLifecycleHook
{
    //public async Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    //{
    //    _ = Task.Run(async () =>
    //    {
    //        await Task.Delay(15000);

    //        var emulatorResources = appModel.Resources.Where(x => x.Annotations.Any(x => x is LambdaEmulatorAnnotation));
    //        foreach (var resource in emulatorResources)
    //        {
    //            await resourceNotificationService.PublishUpdateAsync(resource, s =>
    //            {
    //                if (!string.Equals(s.State?.Text, "Running", StringComparison.InvariantCultureIgnoreCase))
    //                    return s;

    //                return s with
    //                {
    //                    ResourceType = "AWS.Lambda.Emulator",
    //                    State = "Running"
    //                };
    //            });
    //        }

    //        var httpEventSourceResources = appModel.Resources.Where(x => x.Annotations.Any(x => x is HttpEventSourceAnnotation));
    //        foreach (var resource in httpEventSourceResources)
    //        {
    //            await resourceNotificationService.PublishUpdateAsync(resource, s => s with
    //            {
    //                ResourceType = "AWS.Lambda.HttpEndpoint"
    //            });
    //        }

    //        var lambdaFunctionResources = appModel.Resources.Where(x => x.Annotations.Any(x => x is LambdaFunctionAnnotation));
    //        foreach (var resource in lambdaFunctionResources)
    //        {
    //            await resourceNotificationService.PublishUpdateAsync(resource, s => s with
    //            {
    //                ResourceType = "AWS.Lambda.Function"
    //            });
    //        }
    //    });
    //}
}

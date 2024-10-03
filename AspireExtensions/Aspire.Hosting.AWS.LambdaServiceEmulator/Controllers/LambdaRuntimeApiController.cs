using System.Text;
using Aspire.Hosting.AWS.LambdaServiceEmulator.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aspire.Hosting.LambdaServiceEmulator;

public class LambdaRuntimeApiController : ControllerBase
{
    private const string HEADER_BREAK = "-----------------------------------";
    private readonly ILogger<LambdaRuntimeApiController> _logger;
    private readonly ILambdaRuntimeDataStoreManager _runtimeDataStoreManager;

    public LambdaRuntimeApiController(ILogger<LambdaRuntimeApiController> logger, ILambdaRuntimeDataStoreManager runtimeDataStoreManager)
    {
        _logger = logger;
        _runtimeDataStoreManager = runtimeDataStoreManager;
    }

    [HttpPost("/{functionName}/runtime/test-event-sync")]
    public async Task<IActionResult> PostTestEventSync(string functionName)
    {
        _logger.LogDebug("PostTestEventSync: {functionName}", functionName);

        var runtimeDataStore = _runtimeDataStoreManager.GetLambdaRuntimeDataStore(functionName);

        using var reader = new StreamReader(Request.Body);
        var testEvent = await reader.ReadToEndAsync();
        var evnt = runtimeDataStore.QueueEvent(testEvent);
        
        while (evnt.EventStatus != IEventContainer.Status.Success && evnt.EventStatus != IEventContainer.Status.Failure)
        {
            await Task.Delay(50);
        }

        return Ok(evnt.Response);
    }

    [HttpPost("/{functionName}/runtime/test-event")]
    [HttpPost("/{functionName}/2015-03-31/functions/function/invocations")]
    [HttpPost("/2015-03-31/functions/{functionName}/invocations")]
    public async Task<IActionResult> PostTestEvent(string functionName)
    {
        _logger.LogDebug("PostTestEvent: {functionName}", functionName);

        var runtimeDataStore = _runtimeDataStoreManager.GetLambdaRuntimeDataStore(functionName);

        using var reader = new StreamReader(Request.Body);
        var testEvent = await reader.ReadToEndAsync();
        var evnt = runtimeDataStore.QueueEvent(testEvent);

        if (base.HttpContext.Request.Headers.TryGetValue("X-Amz-Invocation-Type", out var invocationType) && 
            string.Equals(invocationType, "RequestResponse", StringComparison.CurrentCultureIgnoreCase))
        {
            while (evnt.EventStatus != IEventContainer.Status.Success && evnt.EventStatus != IEventContainer.Status.Failure)
            {
                // TODO: Use real .NET monitoring APIs for state changes.
                await Task.Delay(50);
            }

            return Ok(evnt.Response);
        }

        return Accepted();
    }
    
    [HttpPost("/{functionName}/2018-06-01/runtime/init/error")]
    public IActionResult PostInitError(string functionName, [FromHeader(Name = "Lambda-Runtime-Function-Error-Type")] string errorType, [FromBody] string error)
    {
        Console.Error.WriteLine("Init Error Type: " + errorType);
        Console.Error.WriteLine(error);
        Console.Error.WriteLine(HEADER_BREAK);
        return Accepted(new StatusResponse{Status = "success"});
    }
    
    [HttpGet("/{functionName}/2018-06-01/runtime/invocation/next")]
    public async Task GetNextInvocation(string functionName,CancellationToken token = default)
    {
        _logger.LogDebug("GetNextInvocation: {functionName}", functionName);

        var runtimeDataStore = _runtimeDataStoreManager.GetLambdaRuntimeDataStore(functionName);

        Console.WriteLine("Function Name: " + functionName);
        IEventContainer? activeEvent;
        while (!runtimeDataStore.TryActivateEvent(out activeEvent) && !token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        if (activeEvent == null)
        {
            Response.StatusCode = 404;
            return;
        }

        Console.WriteLine(HEADER_BREAK);
        Console.WriteLine($"Next invocation returned: {activeEvent.AwsRequestId}");
        
        Response.Headers["Lambda-Runtime-Aws-Request-Id"] = activeEvent.AwsRequestId;
        Response.Headers["Lambda-Runtime-Trace-Id"] = Guid.NewGuid().ToString();
        Response.Headers["Lambda-Runtime-Invoked-Function-Arn"] = activeEvent.FunctionArn;
        Response.StatusCode = 200;

        if (activeEvent.EventJson != null && activeEvent.EventJson.Length > 0)
        {
            // The event is written directly to the response stream to avoid ASP.NET Core attempting any
            // encoding on content passed in the Ok() method.
            Response.Headers["Content-Type"] = "application/json";
            var buffer = UTF8Encoding.UTF8.GetBytes(activeEvent.EventJson);
            await Response.Body.WriteAsync(buffer, 0, buffer.Length);
            Response.Body.Close();
        }
    }
    
    [HttpPost("/{functionName}/2018-06-01/runtime/invocation/{awsRequestId}/response")]
    public async Task<IActionResult> PostInvocationResponse(string functionName,string awsRequestId)
    {
        _logger.LogDebug("PostInvocationResponse: {functionName}", functionName);

        var runtimeDataStore = _runtimeDataStoreManager.GetLambdaRuntimeDataStore(functionName);

        using var reader = new StreamReader(Request.Body);
        var response = await reader.ReadToEndAsync();

        runtimeDataStore.ReportSuccess(awsRequestId, response);
        
        Console.WriteLine(HEADER_BREAK);
        Console.WriteLine($"Response for request {awsRequestId}");
        Console.WriteLine(response);

        return Accepted(new StatusResponse{Status = "success"});
    }

    [HttpPost("/{functionName}/2018-06-01/runtime/invocation/{awsRequestId}/error")]
    public async Task<IActionResult> PostError(string functionName, string awsRequestId, [FromHeader(Name = "Lambda-Runtime-Function-Error-Type")] string errorType)
    {
        _logger.LogDebug("PostError: {functionName}", functionName);

        var runtimeDataStore = _runtimeDataStoreManager.GetLambdaRuntimeDataStore(functionName);
        using var reader = new StreamReader(Request.Body);
        var errorBody = await reader.ReadToEndAsync();

        runtimeDataStore.ReportError(awsRequestId, errorType, errorBody);
        await Console.Error.WriteLineAsync(HEADER_BREAK);
        await Console.Error.WriteLineAsync($"Request {awsRequestId} Error Type: {errorType}");
        await Console.Error.WriteLineAsync(errorBody);
        
        return Accepted(new StatusResponse{Status = "success"});
    }
}

internal class StatusResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("status")]
    public string? Status { get; set; }
}
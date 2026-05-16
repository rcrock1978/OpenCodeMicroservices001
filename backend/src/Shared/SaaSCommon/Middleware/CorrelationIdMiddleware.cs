using System.Diagnostics;

namespace SaaSCommon.Middleware;

/// <summary>
/// Middleware that ensures correlation IDs are propagated across service boundaries
/// for distributed tracing and log correlation.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Invokes the middleware to process the HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers.Append(CorrelationIdHeader, correlationId);
        context.Items[CorrelationIdHeader] = correlationId;

        using var activity = new Activity("RequestPipeline");
        activity.SetTag("correlation.id", correlationId);
        activity.Start();

        await next(context);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using SaaSCommon.Middleware;
using Xunit;

namespace SaaSCommon.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="CorrelationIdMiddleware"/>.
/// </summary>
public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithExistingHeader_PreservesCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "existing-id-123";

        string? capturedCorrelationId = null;
        var next = new RequestDelegate(ctx =>
        {
            capturedCorrelationId = ctx.Items["X-Correlation-ID"]?.ToString();
            return Task.CompletedTask;
        });

        var middleware = new CorrelationIdMiddleware(next);
        await middleware.InvokeAsync(context);

        Assert.Equal("existing-id-123", capturedCorrelationId);
        Assert.Equal("existing-id-123", context.Response.Headers["X-Correlation-ID"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WithoutHeader_GeneratesNewCorrelationId()
    {
        var context = new DefaultHttpContext();

        string? capturedCorrelationId = null;
        var next = new RequestDelegate(ctx =>
        {
            capturedCorrelationId = ctx.Items["X-Correlation-ID"]?.ToString();
            return Task.CompletedTask;
        });

        var middleware = new CorrelationIdMiddleware(next);
        await middleware.InvokeAsync(context);

        Assert.NotNull(capturedCorrelationId);
        Assert.False(string.IsNullOrEmpty(capturedCorrelationId));
        Assert.True(Guid.TryParse(capturedCorrelationId, out _));
        Assert.Equal(capturedCorrelationId, context.Response.Headers["X-Correlation-ID"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_SetsItemInContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = "test-item";

        var next = new RequestDelegate(ctx => Task.CompletedTask);
        var middleware = new CorrelationIdMiddleware(next);
        await middleware.InvokeAsync(context);

        Assert.True(context.Items.ContainsKey("X-Correlation-ID"));
        Assert.Equal("test-item", context.Items["X-Correlation-ID"]);
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;

        var next = new RequestDelegate(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var middleware = new CorrelationIdMiddleware(next);
        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }
}

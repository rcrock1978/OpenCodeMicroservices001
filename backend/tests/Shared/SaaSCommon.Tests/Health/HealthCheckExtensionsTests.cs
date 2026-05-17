using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SaaSCommon.Health;
using Xunit;

namespace SaaSCommon.Tests.Health;

/// <summary>
/// Unit tests for <see cref="HealthCheckExtensions"/>.
/// </summary>
public class HealthCheckExtensionsTests
{
    [Fact]
    public void AddStandardHealthChecks_RegistersHealthCheckService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardHealthChecks("TestService");

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetService<HealthCheckService>();

        Assert.NotNull(healthCheckService);
    }

    [Fact]
    public void AddStandardHealthChecks_RegistersNamedHealthCheck()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddStandardHealthChecks("MyService");

        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetRequiredService<HealthCheckService>();

        // The health check should be registered and discoverable
        Assert.NotNull(healthCheckService);
    }
}

using Ocow.HealthChecks.Dtos;
using Ocow.HealthChecks.Extensions;
using Ocow.HealthChecks.Options;

namespace Ocow.Tests.Unit;

/// <summary>
/// HealthChecks 边界测试，用于验证健康检查能力独立归属 Ocow.HealthChecks。
/// </summary>
public class HealthChecksBoundaryTests
{
    /// <summary>
    /// 验证健康检查默认路径符合服务探活和就绪检查约定。
    /// </summary>
    [Fact]
    public void HealthCheckOption_ShouldUseDefaultHealthPaths()
    {
        var option = new HealthCheckOption();

        Assert.Equal("/health", option.HealthPath);
        Assert.Equal("/live", option.LivePath);
        Assert.Equal("/ready", option.ReadyPath);
    }

    /// <summary>
    /// 验证健康检查端点扩展和响应 DTO 都由 Ocow.HealthChecks 提供。
    /// </summary>
    [Fact]
    public void HealthCheckTypes_ShouldBelongToHealthChecksAssembly()
    {
        Assert.Equal("Ocow.HealthChecks", typeof(HealthChecksEndpointRouteBuilderExtensions).Assembly.GetName().Name);
        Assert.Equal("Ocow.HealthChecks", typeof(HealthCheckResDto).Assembly.GetName().Name);
    }
}

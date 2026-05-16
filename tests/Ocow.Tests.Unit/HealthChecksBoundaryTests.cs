using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Ocow.HealthChecks.Constants;
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

    /// <summary>
    /// 验证依赖健康检查注册后会进入 ready 检查集合。
    /// </summary>
    [Fact]
    public void AddOcowHealthChecks_WhenDependenciesConfigured_ShouldRegisterReadyChecks()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:ConnectionString"] = "Host=localhost;Port=5432;Database=ocow_test;Username=postgres;Password=postgres123",
                ["Redis:Configuration"] = "localhost:6379,abortConnect=false",
                ["RabbitMq:HostName"] = "localhost",
                ["RabbitMq:Port"] = "5672"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddOcowHealthChecks(configuration, "Ocow.Test.Api", checks =>
        {
            checks.AddPostgreSqlCheck(configuration);
            checks.AddRedisCheck(configuration);
            checks.AddRabbitMqCheck(configuration);
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
        var readyCheckNames = options.Registrations
            .Where(registration => registration.Tags.Contains(HealthCheckTags.Ready))
            .Select(registration => registration.Name)
            .ToArray();

        Assert.Contains("postgresql", readyCheckNames);
        Assert.Contains("redis", readyCheckNames);
        Assert.Contains("rabbitmq", readyCheckNames);
    }

    /// <summary>
    /// 验证未配置依赖时不会默认注册 ready 检查，避免服务被未使用依赖拖垮。
    /// </summary>
    [Fact]
    public void AddOcowHealthChecks_WhenDependenciesMissing_ShouldSkipReadyChecks()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();

        services.AddOcowHealthChecks(configuration, "Ocow.Test.Api", checks =>
        {
            checks.AddPostgreSqlCheck(configuration);
            checks.AddRedisCheck(configuration);
            checks.AddRabbitMqCheck(configuration);
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;

        Assert.DoesNotContain(options.Registrations, registration => registration.Tags.Contains(HealthCheckTags.Ready));
    }
}

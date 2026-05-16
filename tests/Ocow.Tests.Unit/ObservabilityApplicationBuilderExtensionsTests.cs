using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.Observability.Extensions;
using Ocow.Observability.Options;

namespace Ocow.Tests.Unit;

/// <summary>
/// 可观测性中间件扩展开关测试，用于验证环境配置能控制请求日志和 Trace 透传能力。
/// </summary>
public class ObservabilityApplicationBuilderExtensionsTests
{
    /// <summary>
    /// 验证关闭请求日志时不会注册 Serilog 请求日志中间件。
    /// </summary>
    [Fact]
    public void UseOcowSerilogRequestLogging_WhenDisabled_ShouldNotRegisterMiddleware()
    {
        var app = CreateApplicationBuilder(new ObservabilityOption
        {
            Logging = new LoggingOption
            {
                EnableRequestLogging = false
            }
        });

        app.UseOcowSerilogRequestLogging();

        Assert.Equal(0, app.UseCallCount);
    }

    /// <summary>
    /// 验证关闭 Trace 透传时不会注册请求 Trace 中间件。
    /// </summary>
    [Fact]
    public void UseOcowRequestTrace_WhenDisabled_ShouldNotRegisterMiddleware()
    {
        var app = CreateApplicationBuilder(new ObservabilityOption
        {
            EnableRequestTrace = false
        });

        app.UseOcowRequestTrace();

        Assert.Equal(0, app.UseCallCount);
    }

    /// <summary>
    /// 创建可计数的测试 ApplicationBuilder。
    /// </summary>
    private static CountingApplicationBuilder CreateApplicationBuilder(ObservabilityOption option)
    {
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(option))
            .BuildServiceProvider();

        return new CountingApplicationBuilder(services);
    }

    private sealed class CountingApplicationBuilder : IApplicationBuilder
    {
        public CountingApplicationBuilder(IServiceProvider applicationServices)
        {
            ApplicationServices = applicationServices;
        }

        public int UseCallCount { get; private set; }

        public IServiceProvider ApplicationServices { get; set; }

        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();

        public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

        /// <summary>
        /// 记录中间件注册次数。
        /// </summary>
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            UseCallCount++;
            return this;
        }

        /// <summary>
        /// 构建空请求管道。
        /// </summary>
        public RequestDelegate Build()
        {
            return _ => Task.CompletedTask;
        }

        /// <summary>
        /// 创建同类型测试 ApplicationBuilder。
        /// </summary>
        public IApplicationBuilder New()
        {
            return new CountingApplicationBuilder(ApplicationServices);
        }
    }
}

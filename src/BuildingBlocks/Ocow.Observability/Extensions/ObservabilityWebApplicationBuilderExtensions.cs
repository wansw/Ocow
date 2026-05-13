using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Observability.Logging;
using Ocow.Observability.Options;
using Serilog;
using Serilog.Formatting.Json;

namespace Ocow.Observability.Extensions;

/// <summary>
/// 可观测性服务注册扩展，用于统一配置结构化日志和服务基础日志属性。
/// </summary>
public static class ObservabilityWebApplicationBuilderExtensions
{
    /// <summary>
    /// 注册 Ocow 基础可观测性能力。
    /// </summary>
    public static WebApplicationBuilder AddOcowObservability(this WebApplicationBuilder builder)
    {
        var serviceName = ResolveServiceName(builder);
        builder.Services.Configure<ObservabilityOption>(option => option.ServiceName = serviceName);

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With(new SensitiveLogEventEnricher())
                .Enrich.WithProperty("ServiceName", serviceName)
                .WriteTo.Console(new JsonFormatter(renderMessage: true));
        });

        return builder;
    }

    /// <summary>
    /// 解析当前服务名称，优先读取 Observability，其次复用 OpenApi 配置。
    /// </summary>
    private static string ResolveServiceName(WebApplicationBuilder builder)
    {
        return builder.Configuration["Observability:ServiceName"]
            ?? builder.Configuration["OpenApi:ServiceName"]
            ?? builder.Environment.ApplicationName;
    }
}

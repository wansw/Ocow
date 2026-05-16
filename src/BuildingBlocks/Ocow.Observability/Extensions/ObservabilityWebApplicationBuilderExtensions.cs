using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Observability.Logging;
using Ocow.Observability.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.OpenTelemetry;

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
        var observabilityOption = ResolveObservabilityOption(builder, serviceName);

        builder.Services.Configure<ObservabilityOption>(options =>
        {
            ConfigurationBinder.Bind(builder.Configuration.GetSection("Observability"), options);
            if (string.IsNullOrWhiteSpace(options.ServiceName))
            {
                options.ServiceName = serviceName;
            }
        });

        ConfigureSerilog(builder, observabilityOption);
        ConfigureOpenTelemetry(builder, observabilityOption);

        return builder;
    }

    /// <summary>
    /// 配置 Serilog 日志框架。
    /// </summary>
    private static void ConfigureSerilog(WebApplicationBuilder builder, ObservabilityOption observabilityOption)
    {
        var loggingOption = observabilityOption.Logging;
        var serviceName = observabilityOption.ServiceName;

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.With(new SensitiveLogEventEnricher())
                .Enrich.WithProperty("ServiceName", serviceName)
                .Enrich.WithProperty("ServiceVersion", observabilityOption.ServiceVersion)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            if (loggingOption.EnableConsole)
            {
                loggerConfiguration.WriteTo.Console(new JsonFormatter(renderMessage: true));
            }

            if (loggingOption.EnableFile)
            {
                loggerConfiguration.WriteTo.File(
                    new JsonFormatter(),
                    loggingOption.FilePath,
                    rollingInterval: ParseRollingInterval(loggingOption.RollingInterval),
                    retainedFileCountLimit: loggingOption.RetainedFileCountLimit,
                    shared: true);
            }

            if (loggingOption.EnableOpenTelemetry)
            {
                loggerConfiguration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = observabilityOption.OtlpEndpoint;
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        { "service.name", serviceName },
                        { "service.version", observabilityOption.ServiceVersion },
                        { "deployment.environment", context.HostingEnvironment.EnvironmentName }
                    };
                });
            }

            SetMinimumLevel(loggerConfiguration, loggingOption.MinimumLevel);
        });
    }

    /// <summary>
    /// 配置 OpenTelemetry。
    /// </summary>
    private static void ConfigureOpenTelemetry(WebApplicationBuilder builder, ObservabilityOption observabilityOption)
    {
        if (!observabilityOption.EnableTracing && !observabilityOption.EnableMetrics)
        {
            return;
        }

        var openTelemetryBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder
                    .AddService(observabilityOption.ServiceName, serviceVersion: observabilityOption.ServiceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        { "deployment.environment", builder.Environment.EnvironmentName }
                    });
            });

        if (observabilityOption.EnableTracing)
        {
            openTelemetryBuilder.WithTracing(tracingBuilder =>
            {
                tracingBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(observabilityOption.OtlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });
        }

        if (observabilityOption.EnableMetrics)
        {
            openTelemetryBuilder.WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(observabilityOption.OtlpEndpoint);
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });
        }
    }

    /// <summary>
    /// 设置最小日志级别。
    /// </summary>
    private static void SetMinimumLevel(LoggerConfiguration loggerConfiguration, string minimumLevel)
    {
        if (Enum.TryParse<LogEventLevel>(minimumLevel, true, out var level))
        {
            loggerConfiguration.MinimumLevel.Is(level);
        }
        else
        {
            loggerConfiguration.MinimumLevel.Information();
        }
    }

    /// <summary>
    /// 解析滚动间隔。
    /// </summary>
    private static RollingInterval ParseRollingInterval(string rollingInterval)
    {
        if (Enum.TryParse<RollingInterval>(rollingInterval, true, out var interval))
        {
            return interval;
        }
        return RollingInterval.Day;
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

    /// <summary>
    /// 解析可观测性配置，并补齐服务名默认值。
    /// </summary>
    private static ObservabilityOption ResolveObservabilityOption(WebApplicationBuilder builder, string serviceName)
    {
        var option = new ObservabilityOption();
        ConfigurationBinder.Bind(builder.Configuration.GetSection("Observability"), option);
        if (string.IsNullOrWhiteSpace(option.ServiceName))
        {
            option.ServiceName = serviceName;
        }

        return option;
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Ocow.HealthChecks.Dtos;
using Ocow.HealthChecks.Options;

namespace Ocow.HealthChecks.Extensions;

/// <summary>
/// 健康检查端点注册扩展。
/// </summary>
public static class HealthChecksEndpointRouteBuilderExtensions
{
    private const string HealthSwaggerGroupName = "Health";

    /// <summary>
    /// 映射 Ocow 健康检查端点。
    /// </summary>
    public static IEndpointRouteBuilder MapOcowHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        var option = endpoints.ServiceProvider.GetRequiredService<IOptions<HealthCheckOption>>().Value;

        endpoints.MapGet(NormalizePath(option.HealthPath), CreateHealthResponseAsync)
            .WithGroupName(HealthSwaggerGroupName)
            .WithSummary("服务健康检查");

        endpoints.MapGet(NormalizePath(option.LivePath), CreateLiveResponse)
            .WithGroupName(HealthSwaggerGroupName)
            .WithSummary("服务存活检查");

        endpoints.MapGet(NormalizePath(option.ReadyPath), CreateHealthResponseAsync)
            .WithGroupName(HealthSwaggerGroupName)
            .WithSummary("服务就绪检查");

        return endpoints;
    }

    /// <summary>
    /// 创建健康检查响应。
    /// </summary>
    private static async Task<IResult> CreateHealthResponseAsync(
        HealthCheckService healthCheckService,
        IOptions<HealthCheckOption> options,
        HttpContext context)
    {
        var report = await healthCheckService.CheckHealthAsync(context.RequestAborted);
        var response = new HealthCheckResDto
        {
            Service = options.Value.ServiceName,
            Status = ToStatusText(report.Status),
            TraceId = context.TraceIdentifier,
            Duration = report.TotalDuration,
            CheckedAt = DateTime.Now,
            Entries = report.Entries.ToDictionary(item => item.Key, item => ToStatusText(item.Value.Status))
        };

        var statusCode = report.Status == HealthStatus.Unhealthy
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status200OK;

        return Results.Json(response, statusCode: statusCode);
    }

    /// <summary>
    /// 创建存活检查响应。
    /// </summary>
    private static IResult CreateLiveResponse(IOptions<HealthCheckOption> options, HttpContext context)
    {
        return Results.Ok(new HealthCheckResDto
        {
            Service = options.Value.ServiceName,
            Status = "ok",
            TraceId = context.TraceIdentifier,
            Duration = TimeSpan.Zero,
            CheckedAt = DateTime.Now
        });
    }

    /// <summary>
    /// 标准化健康检查路径。
    /// </summary>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/health";
        }

        return path.StartsWith("/", StringComparison.Ordinal) ? path : $"/{path}";
    }

    /// <summary>
    /// 转换健康状态为接口输出文本。
    /// </summary>
    private static string ToStatusText(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => "ok",
            HealthStatus.Degraded => "degraded",
            HealthStatus.Unhealthy => "unhealthy",
            _ => status.ToString().ToLowerInvariant()
        };
    }
}

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Ocow.Observability.Options;
using Serilog.Context;

namespace Ocow.Observability.Middleware;

/// <summary>
/// 请求链路中间件，用于生成或透传 TraceId 并写入日志上下文。
/// </summary>
public class RequestTraceMiddleware
{
    /// <summary>
    /// 请求编号请求头名称。
    /// </summary>
    public const string RequestIdHeader = "X-Request-Id";

    /// <summary>
    /// 链路编号请求头名称。
    /// </summary>
    public const string TraceIdHeader = "X-Trace-Id";

    private readonly RequestDelegate _next;
    private readonly ObservabilityOption _option;

    /// <summary>
    /// 创建请求链路中间件。
    /// </summary>
    public RequestTraceMiddleware(RequestDelegate next, IOptions<ObservabilityOption> option)
    {
        _next = next;
        _option = option.Value;
    }

    /// <summary>
    /// 处理当前 HTTP 请求，并把 TraceId 写入响应头和日志上下文。
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = ResolveTraceId(context);

        context.TraceIdentifier = traceId;
        context.Response.Headers[TraceIdHeader] = traceId;
        context.Response.Headers[RequestIdHeader] = traceId;
        Activity.Current?.SetTag("ocow.trace_id", traceId);

        using (LogContext.PushProperty("TraceId", traceId))
        using (LogContext.PushProperty("ServiceName", _option.ServiceName))
        {
            await _next(context);
        }
    }

    /// <summary>
    /// 解析本次请求的 TraceId。
    /// </summary>
    private static string ResolveTraceId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(TraceIdHeader, out var traceHeader) &&
            !string.IsNullOrWhiteSpace(traceHeader))
        {
            return traceHeader.ToString();
        }

        if (context.Request.Headers.TryGetValue(RequestIdHeader, out var requestHeader) &&
            !string.IsNullOrWhiteSpace(requestHeader))
        {
            return requestHeader.ToString();
        }

        var activityTraceId = Activity.Current?.TraceId.ToString();
        return string.IsNullOrWhiteSpace(activityTraceId) ? Guid.NewGuid().ToString("N") : activityTraceId;
    }
}

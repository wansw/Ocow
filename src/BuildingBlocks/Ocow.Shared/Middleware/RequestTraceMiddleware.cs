using Microsoft.AspNetCore.Http;

namespace Ocow.Shared.Middleware;

/// <summary>
/// 请求链路中间件，用于生成或透传 X-Request-Id。
/// </summary>
public class RequestTraceMiddleware
{
    public const string RequestIdHeader = "X-Request-Id";

    private readonly RequestDelegate _next;

    public RequestTraceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 处理当前 HTTP 请求，并把请求编号写入响应头。
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.Request.Headers.TryGetValue(RequestIdHeader, out var headerValue) &&
                        !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = requestId;
        context.Response.Headers[RequestIdHeader] = requestId;

        await _next(context);
    }
}

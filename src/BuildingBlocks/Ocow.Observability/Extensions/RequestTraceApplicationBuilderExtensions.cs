using Microsoft.AspNetCore.Builder;
using Ocow.Observability.Middleware;

namespace Ocow.Observability.Extensions;

/// <summary>
/// 请求链路追踪中间件注册扩展。
/// </summary>
public static class RequestTraceApplicationBuilderExtensions
{
    /// <summary>
    /// 启用请求 TraceId 透传能力。
    /// </summary>
    public static IApplicationBuilder UseOcowRequestTrace(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestTraceMiddleware>();
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.Observability.Middleware;
using Ocow.Observability.Options;

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
        var option = app.ApplicationServices.GetRequiredService<IOptions<ObservabilityOption>>().Value;
        if (!option.EnableRequestTrace)
        {
            return app;
        }

        return app.UseMiddleware<RequestTraceMiddleware>();
    }
}

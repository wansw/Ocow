using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.Observability.Options;
using Serilog;
using Serilog.Events;

namespace Ocow.Observability.Extensions;

/// <summary>
/// Serilog 请求日志中间件扩展，用于输出不包含敏感请求内容的结构化访问日志。
/// </summary>
public static class SerilogRequestLoggingApplicationBuilderExtensions
{
    /// <summary>
    /// 启用 Ocow 结构化请求日志。
    /// </summary>
    public static IApplicationBuilder UseOcowSerilogRequestLogging(this IApplicationBuilder app)
    {
        var option = app.ApplicationServices.GetRequiredService<IOptions<ObservabilityOption>>().Value;
        if (!option.Logging.EnableRequestLogging)
        {
            return app;
        }

       
        return app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = GetRequestLogLevel;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            };
        });
    }

    /// <summary>
    /// 根据响应结果计算请求日志级别。
    /// </summary>
    private static LogEventLevel GetRequestLogLevel(HttpContext httpContext, double elapsed, Exception? exception)
    {
        if (exception is not null || httpContext.Response.StatusCode >= StatusCodes.Status500InternalServerError)
        {
            return LogEventLevel.Error;
        }

        return LogEventLevel.Information;
    }
}

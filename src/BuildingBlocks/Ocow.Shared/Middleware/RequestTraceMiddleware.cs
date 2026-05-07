using Microsoft.AspNetCore.Http;

namespace Ocow.Shared.Middleware;

/// <summary>
/// 请求链路中间件，用于生成或透传 X-Request-Id。  
/// 检查TraceId是否存储不存在就生成新的；放进响应头中；并且把TraceId赋值给HttpContext.TraceIdentifier属性。 
/// 这样就可以在日志中使用TraceIdentifier来关联同一请求的日志记录。
///  </summary>
public class RequestTraceMiddleware
{
    /// <summary>
    /// 请求编号请求头名称。   
    ///  </summary>
    public const string RequestIdHeader = "X-Request-Id";

    private readonly RequestDelegate _next;

    /// <summary>
    /// 创建请求链路中间件。   
    ///  </summary>
    public RequestTraceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 处理当前 HTTP 请求，并把请求编号写入响应头。  
    ///  </summary>
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

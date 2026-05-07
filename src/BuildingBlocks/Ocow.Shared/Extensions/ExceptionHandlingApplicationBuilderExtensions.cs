using Microsoft.AspNetCore.Builder;
using Ocow.Shared.Middleware;

namespace Ocow.Shared.Extensions;

/// <summary>
/// 统一异常处理中间件注册扩展。
/// </summary>
public static class ExceptionHandlingApplicationBuilderExtensions
{
    /// <summary>
    /// 启用统一异常处理，把未处理异常转换为标准接口响应。
    /// </summary>
    public static IApplicationBuilder UseOcowExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

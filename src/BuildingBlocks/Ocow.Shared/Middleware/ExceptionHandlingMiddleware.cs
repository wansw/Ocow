using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocow.Shared.Dtos;
using System.Runtime.ExceptionServices;

namespace Ocow.Shared.Middleware;

/// <summary>
/// 统一异常处理中间件，用于把未处理异常转换成标准接口响应。
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// 创建统一异常处理中间件。
    /// </summary>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// 执行当前 HTTP 请求，并在异常发生时输出统一失败响应。
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// 根据异常类型生成统一错误响应。
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "请求处理发生异常。TraceId：{TraceId}", context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }

        var error = ResolveError(exception);
        var response = ApiResDto<object>.Fail(error.Code, error.Message, context.TraceIdentifier);

        context.Response.Clear();
        context.Response.StatusCode = error.StatusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        await context.Response.WriteAsJsonAsync(response);
    }



    /// <summary>
    /// 把常见异常类型映射成 HTTP 状态码和业务错误码。
    /// </summary>
    private static ErrorInfo ResolveError(Exception exception)
    {
        return exception switch
        {
            ArgumentException => new ErrorInfo(StatusCodes.Status400BadRequest, "BAD_REQUEST", exception.Message),
            InvalidOperationException => new ErrorInfo(StatusCodes.Status400BadRequest, "BUSINESS_ERROR", exception.Message),
            UnauthorizedAccessException => new ErrorInfo(StatusCodes.Status403Forbidden, "FORBIDDEN", exception.Message),
            NotImplementedException => new ErrorInfo(StatusCodes.Status501NotImplemented, "NOT_IMPLEMENTED", exception.Message),
            _ => new ErrorInfo(StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "服务器内部错误。")
        };
    }

    /// <summary>
    /// 实体类，表示错误信息，包括 HTTP 状态码、业务错误码和错误消息。
    /// </summary>
    /// <param name="StatusCode"></param>
    /// <param name="Code"></param>
    /// <param name="Message"></param>
    private sealed record ErrorInfo(int StatusCode, string Code, string Message);
}

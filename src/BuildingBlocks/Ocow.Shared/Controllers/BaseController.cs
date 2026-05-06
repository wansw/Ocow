using Microsoft.AspNetCore.Mvc;
using Ocow.Shared.Dtos;

namespace Ocow.Shared.Controllers;

/// <summary>
/// API 控制器基类，用于统一封装成功、失败和 404 响应。
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// 返回统一成功响应，并自动写入当前请求 TraceId。
    /// </summary>
    protected ApiResDto<T> Success<T>(T data)
    {
        return ApiResDto<T>.Ok(data, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 返回布尔成功响应，并自动写入当前请求 TraceId。
    /// </summary>
    protected ApiResDto<bool> Success()
    {
        return ApiResDto<bool>.Ok(true, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 返回统一失败响应，并自动写入当前请求 TraceId。
    /// </summary>
    protected ApiResDto<T> Fail<T>(string code, string message)
    {
        return ApiResDto<T>.Fail(code, message, HttpContext.TraceIdentifier);
    }

    /// <summary>
    /// 返回统一 404 响应，并自动写入当前请求 TraceId。
    /// </summary>
    protected ActionResult<ApiResDto<T>> NotFoundRes<T>(string code, string message)
    {
        return NotFound(Fail<T>(code, message));
    }
}

namespace Ocow.Shared.Dtos;

/// <summary>
/// 统一接口响应 DTO，用于包装业务数据、错误码和链路追踪编号。
/// </summary>
public class ApiResDto<T>
{
    public bool Success { get; init; }

    public string Code { get; init; } = "0";

    public string Message { get; init; } = "success";

    public T? Data { get; init; }

    public string? TraceId { get; init; }

    /// <summary>
    /// 创建成功响应，统一返回给 Controller。
    /// </summary>
    public static ApiResDto<T> Ok(T data, string? traceId = null)
    {
        return new ApiResDto<T>
        {
            Success = true,
            Data = data,
            TraceId = traceId
        };
    }

    /// <summary>
    /// 创建失败响应，统一表达业务错误。
    /// </summary>
    public static ApiResDto<T> Fail(string code, string message, string? traceId = null)
    {
        return new ApiResDto<T>
        {
            Success = false,
            Code = code,
            Message = message,
            TraceId = traceId
        };
    }
}

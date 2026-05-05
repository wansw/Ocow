namespace Ocow.Shared.Dtos;

/// <summary>
/// 统一接口响应 DTO，用于包装业务数据、错误码和链路追踪编号。/// </summary>
public class ApiResDto<T>
{
    /// <summary>
    /// 是否处理成功。    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 业务响应编码，成功时默认。0。    /// </summary>
    public string Code { get; init; } = "0";

    /// <summary>
    /// 响应消息，用于展示成功或失败原因。    /// </summary>
    public string Message { get; init; } = "success";

    /// <summary>
    /// 响应业务数据。    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// 请求链路编号，用于日志追踪。    /// </summary>
    public string? TraceId { get; init; }

    /// <summary>
    /// 创建成功响应，统一返回。Controller。    /// </summary>
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
    /// 创建失败响应，统一表达业务错误。    /// </summary>
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

namespace Ocow.WeChat.Core.Exceptions;

/// <summary>
/// 微信开放接口异常，用于保留微信错误码和错误说明。
/// </summary>
public class WechatApiException : Exception
{
    /// <summary>
    /// 创建微信开放接口异常。
    /// </summary>
    public WechatApiException(int errCode, string? errMsg)
        : base($"微信接口调用失败：{errCode}，{errMsg}")
    {
        ErrCode = errCode;
        ErrMsg = errMsg;
    }

    /// <summary>
    /// 微信错误码。
    /// </summary>
    public int ErrCode { get; }

    /// <summary>
    /// 微信错误说明。
    /// </summary>
    public string? ErrMsg { get; }
}

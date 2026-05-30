using System.ComponentModel.DataAnnotations;

namespace Ocow.WeChat.Abstractions.Dtos;

/// <summary>
/// 小程序登录凭证校验请求 DTO，用于使用 wx.login 返回的 code 换取微信身份。
/// </summary>
public class WechatCode2SessionReqDto
{
    /// <summary>
    /// 小程序登录时 wx.login 返回的临时 code。
    /// </summary>
    [Required(ErrorMessage = "微信登录 code 不能为空")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 可选的小程序 AppId，未传时使用服务配置的默认 AppId。
    /// </summary>
    public string? AppId { get; init; }
}

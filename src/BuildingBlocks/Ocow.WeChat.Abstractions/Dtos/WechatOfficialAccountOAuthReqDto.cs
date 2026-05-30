using System.ComponentModel.DataAnnotations;

namespace Ocow.WeChat.Abstractions.Dtos;

/// <summary>
/// 公众号网页授权请求 DTO，用于使用授权回调 code 换取公众号用户身份。
/// </summary>
public class WechatOfficialAccountOAuthReqDto
{
    /// <summary>
    /// 公众号网页授权回调返回的临时 code。
    /// </summary>
    [Required(ErrorMessage = "公众号授权 code 不能为空")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 可选的公众号 AppId，未传时使用服务配置的默认 AppId。
    /// </summary>
    public string? AppId { get; init; }
}

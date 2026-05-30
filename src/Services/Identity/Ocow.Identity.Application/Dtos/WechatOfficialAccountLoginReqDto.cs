using System.ComponentModel.DataAnnotations;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 公众号微信登录请求 DTO，用于使用公众号网页授权 code 登录系统。
/// </summary>
public class WechatOfficialAccountLoginReqDto
{
    /// <summary>
    /// 公众号网页授权回调返回的临时 code。
    /// </summary>
    [Required(ErrorMessage = "不能为空")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 公众号网页授权回调带回的 state，用于防止伪造回调和重复提交。
    /// </summary>
    [Required(ErrorMessage = "不能为空")]
    public string State { get; init; } = string.Empty;
}

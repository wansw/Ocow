using System.ComponentModel.DataAnnotations;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 小程序微信登录请。DTO。/// </summary>
public class WechatLoginReqDto
{
    /// <summary>
    /// 微信小程序登。code。    
    /// </summary>
    [Required(ErrorMessage = "不能为空")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 调试或微信服务返回的 openid。    
    /// </summary>
    public string? OpenId { get; init; }

    /// <summary>
    /// 微信 unionid。    
    /// </summary>
    public string? UnionId { get; init; }
}

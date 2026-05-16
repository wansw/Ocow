using System.ComponentModel;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 后台管理员登录请。DTO
/// </summary>
public class AdminLoginReqDto
{
    /// <summary>
    /// 管理员用户名。    
    /// </summary>
    [DefaultValue("admin")]
    public string UserName { get; init; }

    /// <summary>
    /// 管理员登录密码。     Admin@123456 / admin
    /// </summary>
    [DefaultValue("Admin@123456")]
    public string Password { get; init; }
}

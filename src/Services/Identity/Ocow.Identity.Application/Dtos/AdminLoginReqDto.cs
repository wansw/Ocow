namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 后台管理员登录请求 DTO。
/// </summary>
public class AdminLoginReqDto
{
    /// <summary>
    /// 管理员用户名。
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// 管理员登录密码。
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 后台管理员登录请求 DTO。
/// </summary>
public class AdminLoginReqDto
{
    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

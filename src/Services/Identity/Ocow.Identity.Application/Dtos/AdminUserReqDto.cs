namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 管理员保存请求 DTO。
/// </summary>
public class AdminUserReqDto
{
    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public List<Guid> RoleIds { get; init; } = new();
}

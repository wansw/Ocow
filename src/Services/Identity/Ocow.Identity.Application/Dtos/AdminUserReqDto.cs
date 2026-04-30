namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 管理员保存请求 DTO。
/// </summary>
public class AdminUserReqDto
{
    /// <summary>
    /// 管理员用户名。
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// 管理员初始密码。
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// 管理员展示名称。
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// 绑定的角色编号列表。
    /// </summary>
    public List<Guid> RoleIds { get; init; } = new();
}

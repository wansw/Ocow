using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 管理员响应 DTO。
/// </summary>
public class AdminUserResDto
{
    /// <summary>
    /// 管理员编号。
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 管理员用户名。
    /// </summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>
    /// 管理员展示名称。
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// 管理员账号状态。
    /// </summary>
    public AdminUserStatusEnum Status { get; init; }
}

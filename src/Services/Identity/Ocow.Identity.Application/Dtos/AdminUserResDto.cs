using Ocow.Identity.Domain.Enums;

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 管理员响应 DTO。
/// </summary>
public class AdminUserResDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public AdminUserStatusEnum Status { get; init; }
}

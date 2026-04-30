namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 权限点响应 DTO。
/// </summary>
public class PermissionResDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Module { get; init; } = string.Empty;
}

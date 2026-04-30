namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 角色响应 DTO。
/// </summary>
public class RoleResDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

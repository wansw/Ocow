namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 角色保存请求 DTO。
/// </summary>
public class RoleReqDto
{
    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
}

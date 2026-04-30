namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 角色保存请求 DTO。
/// </summary>
public class RoleReqDto
{
    /// <summary>
    /// 角色编码。
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;
}

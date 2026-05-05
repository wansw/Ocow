namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 角色响应 DTO。/// </summary>
public class RoleResDto
{
    /// <summary>
    /// 角色编号。    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 角色编码。    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 角色名称。    /// </summary>
    public string Name { get; init; } = string.Empty;
}

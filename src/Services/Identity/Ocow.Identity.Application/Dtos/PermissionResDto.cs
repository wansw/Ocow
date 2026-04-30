namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 权限点响应 DTO。
/// </summary>
public class PermissionResDto
{
    /// <summary>
    /// 权限点编号。
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// 权限点编码。
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// 权限点名称。
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// 权限点所属模块。
    /// </summary>
    public string Module { get; init; } = string.Empty;
}

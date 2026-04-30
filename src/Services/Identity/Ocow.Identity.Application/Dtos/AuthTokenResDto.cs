namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 登录 Token 响应 DTO。
/// </summary>
public class AuthTokenResDto
{
    /// <summary>
    /// 访问 Token。
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// 刷新 Token。
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// 访问 Token 过期时间。
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Token 使用范围。
    /// </summary>
    public string Scope { get; init; } = string.Empty;

    /// <summary>
    /// 当前身份拥有的权限点。
    /// </summary>
    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
}

namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 登录 Token 响应 DTO。
/// </summary>
public class AuthTokenResDto
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime ExpiresAt { get; init; }

    public string Scope { get; init; } = string.Empty;

    public IReadOnlyList<string> Permissions { get; init; } = Array.Empty<string>();
}

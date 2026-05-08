namespace Ocow.Identity.Application.Models;

/// <summary>
/// 刷新 Token 会话缓存模型，用于关联 RefreshToken 和当前 Admin JWT 会话。
/// </summary>
public class RefreshTokenSession
{
    /// <summary>
    /// 登录主体编号。
    /// </summary>
    public Guid SubjectId { get; init; }

    /// <summary>
    /// 当前会话编号。
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// 当前 JWT 编号。
    /// </summary>
    public string JwtId { get; init; } = string.Empty;

    /// <summary>
    /// 当前访问 Token 过期时间。
    /// </summary>
    public DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// 刷新 Token 过期时间。
    /// </summary>
    public DateTime RefreshTokenExpiresAt { get; init; }
}

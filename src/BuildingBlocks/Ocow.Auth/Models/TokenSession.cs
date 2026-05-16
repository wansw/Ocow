namespace Ocow.Auth.Models;

/// <summary>
/// Token 会话模型，用于保存 Admin JWT 对应的 Redis 集中式会话状态。
/// </summary>
public class TokenSession
{
    /// <summary>
    /// 会话编号。
    /// </summary>
    public Guid SessionId { get; init; }

    /// <summary>
    /// 登录主体编号。
    /// </summary>
    public Guid SubjectId { get; init; }

    /// <summary>
    /// Token 使用范围。
    /// </summary>
    public string Scope { get; init; } = string.Empty;

    /// <summary>
    /// JWT 编号。
    /// </summary>
    public string JwtId { get; init; } = string.Empty;

    /// <summary>
    /// 会话过期时间。
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}

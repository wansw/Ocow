namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 刷新 Token 请求 DTO。/// </summary>
public class RefreshTokenReqDto
{
    /// <summary>
    /// 刷新 Token。    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

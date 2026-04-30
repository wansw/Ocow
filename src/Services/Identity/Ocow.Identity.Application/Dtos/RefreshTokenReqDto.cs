namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 刷新 Token 请求 DTO。
/// </summary>
public class RefreshTokenReqDto
{
    public string RefreshToken { get; init; } = string.Empty;
}

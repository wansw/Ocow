namespace Ocow.Identity.Application.Dtos;

/// <summary>
/// 小程序当前身份响应 DTO。
/// </summary>
public class ClientProfileResDto
{
    public Guid MemberId { get; init; }

    public string OpenId { get; init; } = string.Empty;
}

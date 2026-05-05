namespace Ocow.Identity.Domain.Enums;

/// <summary>
/// Token 使用范围枚举，用于区分小程序、后台和内部服务身份。/// </summary>
public enum TokenScopeEnum
{
    Client = 1,
    Admin = 2,
    Internal = 3
}

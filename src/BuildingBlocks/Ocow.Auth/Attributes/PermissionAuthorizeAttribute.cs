using Microsoft.AspNetCore.Authorization;

namespace Ocow.Auth.Attributes;

/// <summary>
/// 权限点授权特性，用于声明接口需要的后台权限点编码。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    /// <summary>
    /// 创建权限点授权特性。
    /// </summary>
    public PermissionAuthorizeAttribute(string permissionCode)
    {
        Policy = $"{PolicyPrefix}{permissionCode}";
    }
}

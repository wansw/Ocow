using Microsoft.AspNetCore.Authorization;

namespace Ocow.InternalAuth.Requirements;

/// <summary>
/// 权限点授权要求，用于表达当前接口需要的后台权限点编码。
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 创建权限点授权要求。
    /// </summary>
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    /// <summary>
    /// 权限点编码。
    /// </summary>
    public string PermissionCode { get; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Ocow.InternalAuth.Attributes;
using Ocow.InternalAuth.Extensions;
using Ocow.InternalAuth.Requirements;

namespace Ocow.InternalAuth.Services;

/// <summary>
/// 权限点授权策略提供器，用于把 Permission 前缀策略动态转换为权限点授权策略。
/// </summary>
public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    /// <summary>
    /// 创建权限点授权策略提供器。
    /// </summary>
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    /// <summary>
    /// 根据策略名称解析权限点授权策略。
    /// </summary>
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PermissionAuthorizeAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return await base.GetPolicyAsync(policyName);
        }

        var permissionCode = policyName[PermissionAuthorizeAttribute.PolicyPrefix.Length..];
        return new AuthorizationPolicyBuilder(InternalAuthServiceCollectionExtensions.AdminJwtScheme)
            .RequireAuthenticatedUser()
            .RequireClaim("scope", "admin")
            .AddRequirements(new PermissionRequirement(permissionCode))
            .Build();
    }
}

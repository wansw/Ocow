using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Ocow.Auth.Attributes;
using Ocow.Auth.Extensions;
using Ocow.Auth.Options;
using Ocow.Auth.Requirements;

namespace Ocow.Auth.Services;

/// <summary>
/// 权限点授权策略提供器，用于把 Permission 前缀策略动态转换为权限点授权策略。
/// </summary>
public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly GatewayForwardedJwtOption _gatewayOption;

    /// <summary>
    /// 创建权限点授权策略提供器。
    /// </summary>
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IOptions<GatewayForwardedJwtOption> gatewayOption)
        : base(options)
    {
        _gatewayOption = gatewayOption.Value;
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
        return new AuthorizationPolicyBuilder(AuthServiceCollectionExtensions.GetAdminSchemes(_gatewayOption).ToArray())
            .RequireAuthenticatedUser()
            .RequireClaim("scope", "admin")
            .AddRequirements(new PermissionRequirement(permissionCode))
            .Build();
    }
}

using Microsoft.AspNetCore.Authorization;
using Ocow.InternalAuth.Requirements;

namespace Ocow.InternalAuth.Services;

/// <summary>
/// 权限点授权处理器，用于根据 Admin JWT 中的 permission 声明校验接口访问权限。
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    /// <summary>
    /// 处理权限点授权校验。
    /// </summary>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(x =>
            string.Equals(x.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.Value, requirement.PermissionCode, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

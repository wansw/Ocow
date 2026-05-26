using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.Auth.Attributes;
using Ocow.Auth.Extensions;

namespace Ocow.Tests.Unit;

/// <summary>
/// 网关转发 JWT 认证测试，用于验证微服务生产环境只信任网关转发身份，开发环境可兼容 Swagger 直连调试。
/// </summary>
public class GatewayForwardedJwtAuthTests
{
    /// <summary>
    /// 验证启用网关转发模式后客户策略默认只接受网关内部用户 JWT。
    /// </summary>
    [Fact]
    public async Task CustomerPolicy_WhenGatewayForwardedJwtEnabled_ShouldUseGatewayUserJwtOnly()
    {
        var policy = await GetPolicyAsync(AuthServiceCollectionExtensions.CustomerOnlyPolicy, allowDirectIdentityJwt: false);

        Assert.Contains(AuthServiceCollectionExtensions.GatewayUserJwtScheme, policy.AuthenticationSchemes);
        Assert.DoesNotContain(AuthServiceCollectionExtensions.CustomerJwtScheme, policy.AuthenticationSchemes);
    }

    /// <summary>
    /// 验证开发环境打开直连兼容后客户策略同时接受网关内部 JWT 和原始客户 JWT。
    /// </summary>
    [Fact]
    public async Task CustomerPolicy_WhenDirectIdentityJwtAllowed_ShouldUseGatewayAndCustomerJwt()
    {
        var policy = await GetPolicyAsync(AuthServiceCollectionExtensions.CustomerOnlyPolicy, allowDirectIdentityJwt: true);

        Assert.Contains(AuthServiceCollectionExtensions.GatewayUserJwtScheme, policy.AuthenticationSchemes);
        Assert.Contains(AuthServiceCollectionExtensions.CustomerJwtScheme, policy.AuthenticationSchemes);
    }

    /// <summary>
    /// 验证权限点策略在生产网关转发模式下不再接受原始后台 JWT。
    /// </summary>
    [Fact]
    public async Task PermissionPolicy_WhenGatewayForwardedJwtEnabled_ShouldUseGatewayUserJwtOnly()
    {
        var policy = await GetPolicyAsync($"{PermissionAuthorizeAttribute.PolicyPrefix}order.ship", allowDirectIdentityJwt: false);

        Assert.Contains(AuthServiceCollectionExtensions.GatewayUserJwtScheme, policy.AuthenticationSchemes);
        Assert.DoesNotContain(AuthServiceCollectionExtensions.AdminJwtScheme, policy.AuthenticationSchemes);
    }

    private static async Task<AuthorizationPolicy> GetPolicyAsync(string policyName, bool allowDirectIdentityJwt)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "Ocow.Identity",
                ["Jwt:Audience"] = "Ocow.Clients",
                ["Jwt:Secret"] = "UnitTestIdentityJwtSecret@2026-EnoughLong",
                ["GatewayForwardedJwt:Enabled"] = "true",
                ["GatewayForwardedJwt:AllowDirectIdentityJwt"] = allowDirectIdentityJwt.ToString(),
                ["GatewayForwardedJwt:Issuer"] = "Ocow.Gateway",
                ["GatewayForwardedJwt:Audience"] = "Ocow.Microservices",
                ["GatewayForwardedJwt:Secret"] = "UnitTestGatewayForwardedJwtSecret@2026-EnoughLong"
            })
            .Build();

        services.AddOptions();
        services.AddLogging();
        services.AddOcowAuth(configuration);

        using var provider = services.BuildServiceProvider();
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        return await policyProvider.GetPolicyAsync(policyName) ?? throw new InvalidOperationException("未找到授权策略。");
    }
}

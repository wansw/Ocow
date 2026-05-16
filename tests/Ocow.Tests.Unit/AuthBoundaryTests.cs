using Ocow.Auth.Attributes;
using Ocow.Auth.Extensions;
using Ocow.Auth.Interfaces;
using Ocow.Auth.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// Auth 边界测试，用于验证用户侧 JWT、权限点授权和 Admin Token 会话校验归属 Ocow.Auth。
/// </summary>
public class AuthBoundaryTests
{
    /// <summary>
    /// 验证权限点授权特性由 Ocow.Auth 提供并生成动态授权策略名。
    /// </summary>
    [Fact]
    public void PermissionAuthorizeAttribute_ShouldBelongToAuthAndBuildPolicyName()
    {
        var attribute = new PermissionAuthorizeAttribute("identity.role.read");

        Assert.Equal("Ocow.Auth", typeof(PermissionAuthorizeAttribute).Assembly.GetName().Name);
        Assert.Equal($"{PermissionAuthorizeAttribute.PolicyPrefix}identity.role.read", attribute.Policy);
    }

    /// <summary>
    /// 验证 Admin/Customer JWT 策略和 Admin Token 会话校验由 Ocow.Auth 提供。
    /// </summary>
    [Fact]
    public void AdminCustomerJwtAuthorization_ShouldBelongToAuth()
    {
        Assert.Equal("AdminJwt", AuthServiceCollectionExtensions.AdminJwtScheme);
        Assert.Equal("CustomerJwt", AuthServiceCollectionExtensions.CustomerJwtScheme);
        Assert.Equal("AdminOnly", AuthServiceCollectionExtensions.AdminOnlyPolicy);
        Assert.Equal("CustomerOnly", AuthServiceCollectionExtensions.CustomerOnlyPolicy);
        Assert.Equal("Ocow.Auth", typeof(IAdminTokenSessionValidator).Assembly.GetName().Name);
        Assert.Equal("Ocow.Auth", typeof(RedisAdminTokenSessionValidator).Assembly.GetName().Name);
    }
}

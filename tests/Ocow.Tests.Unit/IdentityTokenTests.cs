using Microsoft.Extensions.Options;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// 身份认证单元测试，用于验证密码摘要和 Token 签发规则。/// </summary>
public class IdentityTokenTests
{
    /// <summary>
    /// 验证密码摘要可以正确校验明文密码。    /// </summary>
    [Fact]
    public void Verify_WhenPasswordMatches_ShouldReturnTrue()
    {
        var hash = PasswordHashService.Hash("Ocow@2026");

        var result = PasswordHashService.Verify("Ocow@2026", hash);

        Assert.True(result);
    }

    /// <summary>
    /// 验证签发 Admin Token 时会带上权限点。    /// </summary>
    [Fact]
    public void IssueToken_WhenAdmin_ShouldReturnPermissions()
    {
        var service = new TokenService(Options.Create(new JwtTokenOption
        {
            Secret = "UnitTestIdentityJwtSecret@2026-EnoughLong"
        }));

        var token = service.IssueToken(Guid.NewGuid(), "admin", new[] { "order.ship" });

        Assert.Equal("admin", token.Scope);
        Assert.Contains("order.ship", token.Permissions);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
    }
}

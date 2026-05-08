using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Ocow.Identity.Application.Options;
using Ocow.Identity.Application.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// 后台 Token 会话测试，用于验证 Admin JWT 支持 Redis 集中式会话所需的标识。
/// </summary>
public class AdminTokenSessionTests
{
    /// <summary>
    /// 验证签发 Admin JWT 时会写入会话编号和 JWT 编号。
    /// </summary>
    [Fact]
    public void IssueToken_WhenAdminSessionProvided_ShouldContainSidAndJti()
    {
        var sessionId = Guid.NewGuid();
        var jwtId = Guid.NewGuid().ToString("N");
        var service = new TokenService(Options.Create(new JwtTokenOption
        {
            Secret = "UnitTestIdentityJwtSecret@2026-EnoughLong"
        }));

        var token = service.IssueToken(Guid.NewGuid(), "admin", Array.Empty<string>(), sessionId: sessionId, jwtId: jwtId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        Assert.Equal(sessionId.ToString(), jwt.Claims.Single(x => x.Type == "sid").Value);
        Assert.Equal(jwtId, jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value);
    }
}

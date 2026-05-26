using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ocow.InternalAuth.Interfaces;
using Ocow.InternalAuth.Options;

namespace Ocow.InternalAuth.Services;

/// <summary>
/// 内部服务 Token 签发器，用于生成带 internal scope 的服务 JWT。
/// </summary>
public class InternalServiceTokenProvider : IInternalServiceTokenProvider
{
    private readonly InternalAuthOption _option;

    /// <summary>
    /// 初始化内部服务 Token 签发器。
    /// </summary>
    public InternalServiceTokenProvider(IOptions<InternalAuthOption> option)
    {
        _option = option.Value;
    }

    /// <summary>
    /// 创建当前服务调用内部接口使用的 JWT。
    /// </summary>
    public string CreateToken()
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(Math.Max(1, _option.TokenExpireMinutes));
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_option.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, _option.ServiceName),
            new Claim("service_name", _option.ServiceName),
            new Claim("scope", "internal")
        };
        var token = new JwtSecurityToken(_option.Issuer, _option.Audience, claims, now, expires, credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

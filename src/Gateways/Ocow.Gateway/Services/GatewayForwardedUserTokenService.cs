using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ocow.Auth.Options;

namespace Ocow.Gateway.Services;

/// <summary>
/// 网关转发用户 Token 服务，用于签发下游微服务信任的短期内部用户 JWT。
/// </summary>
public sealed class GatewayForwardedUserTokenService : IGatewayForwardedUserTokenService
{
    private static readonly HashSet<string> ReservedClaimTypes =
    [
        JwtRegisteredClaimNames.Aud,
        JwtRegisteredClaimNames.Exp,
        JwtRegisteredClaimNames.Iat,
        JwtRegisteredClaimNames.Iss,
        JwtRegisteredClaimNames.Jti,
        JwtRegisteredClaimNames.Nbf
    ];

    private readonly GatewayForwardedJwtOption _option;

    /// <summary>
    /// 创建网关转发用户 Token 服务。
    /// </summary>
    public GatewayForwardedUserTokenService(IOptions<GatewayForwardedJwtOption> option)
    {
        _option = option.Value;
    }

    /// <summary>
    /// 根据当前用户身份创建网关内部转发 Token。
    /// </summary>
    public string CreateToken(ClaimsPrincipal principal)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_option.TokenExpireMinutes);
        var claims = principal.Claims
            .Where(claim => !ReservedClaimTypes.Contains(claim.Type))
            .Select(claim => new Claim(claim.Type, claim.Value))
            .ToList();

        var sourceJwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (!string.IsNullOrWhiteSpace(sourceJwtId))
        {
            claims.Add(new Claim("source_jti", sourceJwtId));
        }

        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
        claims.Add(new Claim("gateway_name", _option.GatewayName));
        claims.Add(new Claim("token_use", "gateway_forwarded_user"));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_option.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _option.Issuer,
            audience: _option.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

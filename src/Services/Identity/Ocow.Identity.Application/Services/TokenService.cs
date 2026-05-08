using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ocow.Identity.Application.Dtos;
using Ocow.Identity.Application.Interfaces;
using Ocow.Identity.Application.Options;

namespace Ocow.Identity.Application.Services;

/// <summary>
/// Token 服务实现，用于签发 Customer JWT 和 Admin JWT。
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtTokenOption _option;

    /// <summary>
    /// 创建 Token 服务。
    /// </summary>
    public TokenService(IOptions<JwtTokenOption> option)
    {
        _option = option.Value;
    }

    /// <summary>
    /// 签发登录 Token。
    /// </summary>
    public AuthTokenResDto IssueToken(Guid subjectId,string scope,IEnumerable<string> permissions,
        IEnumerable<Claim>? extraClaims = null,Guid? sessionId = null,string? jwtId = null)
    {
        var permissionList = permissions.Distinct().ToList();
        var expiresAt = DateTime.UtcNow.AddMinutes(_option.AccessTokenMinutes);
        var tokenSessionId = sessionId ?? Guid.NewGuid();
        var tokenJwtId = string.IsNullOrWhiteSpace(jwtId) ? Guid.NewGuid().ToString("N") : jwtId;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new(JwtRegisteredClaimNames.Jti, tokenJwtId),
            new("sid", tokenSessionId.ToString()),
            new("scope", scope)
        };

        claims.AddRange(permissionList.Select(permission => new Claim("permission", permission)));
        if (extraClaims is not null)
        {
            claims.AddRange(extraClaims);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_option.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_option.Issuer, _option.Audience, claims, expires: expiresAt, signingCredentials: credentials);

        return new AuthTokenResDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)),
            ExpiresAt = expiresAt,
            Scope = scope,
            Permissions = permissionList
        };
    }
}

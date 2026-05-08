using System.Security.Claims;
using Ocow.Identity.Application.Dtos;

namespace Ocow.Identity.Application.Interfaces;

/// <summary>
/// Token 服务接口，用于签发访。Token 和刷。Token。/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 签发登录 Token。    /// </summary>
    AuthTokenResDto IssueToken(
        Guid subjectId,
        string scope,
        IEnumerable<string> permissions,
        IEnumerable<Claim>? extraClaims = null,
        Guid? sessionId = null,
        string? jwtId = null);
}

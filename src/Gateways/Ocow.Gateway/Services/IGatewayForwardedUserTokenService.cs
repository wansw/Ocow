using System.Security.Claims;

namespace Ocow.Gateway.Services;

/// <summary>
/// 网关转发用户 Token 服务接口，用于把已验证的外部用户身份签发为内部用户 JWT。
/// </summary>
public interface IGatewayForwardedUserTokenService
{
    /// <summary>
    /// 根据当前用户身份创建网关内部转发 Token。
    /// </summary>
    string CreateToken(ClaimsPrincipal principal);
}

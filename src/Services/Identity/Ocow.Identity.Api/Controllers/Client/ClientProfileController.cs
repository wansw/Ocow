using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.InternalAuth.Extensions;
using Ocow.Identity.Application.Dtos;
using Ocow.Shared.Controllers;
using Ocow.Shared.Dtos;

namespace Ocow.Identity.Api.Controllers.Client;

/// <summary>
/// 小程序当前身份接口，用于查询当前登录会员身份。/// </summary>

[Route("api/auth")]
[Authorize(Policy = InternalAuthServiceCollectionExtensions.CustomerOnlyPolicy)]
[Tags("小程序身份")]
public class ClientProfileController : BaseController
{
    /// <summary>
    /// 查询当前小程序用户身份。    /// </summary>
    [HttpGet("me")]
    public ApiResDto<ClientProfileResDto> GetMe()
    {
        var memberId = User.FindFirst("memberId")?.Value;
        var openId = User.FindFirst("openid")?.Value ?? string.Empty;

        var result = new ClientProfileResDto
        {
            MemberId = Guid.TryParse(memberId, out var id) ? id : Guid.Empty,
            OpenId = openId
        };

        return Success(result);
    }
}

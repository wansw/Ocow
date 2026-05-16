using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.AspNetCore.SwaggerApi;
using Ocow.Auth.Extensions;

namespace Ocow.AspNetCore.Controllers;

/// <summary>
/// 客户端接口控制器基类，用于统一客户端 Swagger 分组和 Customer 授权策略。
/// </summary>
[ApiExplorerSettings(GroupName = SwaggerApiGroupNames.Client)]
[Authorize(Policy = AuthServiceCollectionExtensions.CustomerOnlyPolicy)]
public abstract class ClientController : BaseController
{
}

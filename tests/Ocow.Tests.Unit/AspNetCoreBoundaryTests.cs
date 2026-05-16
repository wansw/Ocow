using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.AspNetCore.Controllers;
using Ocow.AspNetCore.SwaggerApi;
using Ocow.Auth.Extensions;
using Ocow.Shared.Dtos;

namespace Ocow.Tests.Unit;

/// <summary>
/// AspNetCore 边界测试，用于验证 Web API 通用能力从 Shared 拆分到 Ocow.AspNetCore。
/// </summary>
public class AspNetCoreBoundaryTests
{
    /// <summary>
    /// 验证 Shared 只保留基础 DTO，不再引用 ASP.NET Core 或 Swagger。
    /// </summary>
    [Fact]
    public void SharedAssembly_ShouldNotReferenceAspNetCoreOrSwagger()
    {
        var references = typeof(ApiResDto<>).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();

        Assert.DoesNotContain(references, x => x is not null && x.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, x => x is not null && x.StartsWith("Swashbuckle", StringComparison.Ordinal));
    }

    /// <summary>
    /// 验证后台 Controller 基类带有后台分组和 Admin 授权策略。
    /// </summary>
    [Fact]
    public void AdminController_ShouldDefineAdminBoundary()
    {
        var apiExplorer = typeof(AdminController).GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), true)
            .OfType<ApiExplorerSettingsAttribute>()
            .Single();
        var authorize = typeof(AdminController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .OfType<AuthorizeAttribute>()
            .Single();

        Assert.True(typeof(BaseController).IsAssignableFrom(typeof(AdminController)));
        Assert.Equal(SwaggerApiGroupNames.Admin, apiExplorer.GroupName);
        Assert.Equal(AuthServiceCollectionExtensions.AdminOnlyPolicy, authorize.Policy);
    }

    /// <summary>
    /// 验证客户端 Controller 基类带有客户端分组和 Customer 授权策略。
    /// </summary>
    [Fact]
    public void ClientController_ShouldDefineClientBoundary()
    {
        var apiExplorer = typeof(ClientController).GetCustomAttributes(typeof(ApiExplorerSettingsAttribute), true)
            .OfType<ApiExplorerSettingsAttribute>()
            .Single();
        var authorize = typeof(ClientController).GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .OfType<AuthorizeAttribute>()
            .Single();

        Assert.True(typeof(BaseController).IsAssignableFrom(typeof(ClientController)));
        Assert.Equal(SwaggerApiGroupNames.Client, apiExplorer.GroupName);
        Assert.Equal(AuthServiceCollectionExtensions.CustomerOnlyPolicy, authorize.Policy);
    }
}

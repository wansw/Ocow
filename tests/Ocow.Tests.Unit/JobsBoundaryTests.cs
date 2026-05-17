using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocow.Auth.Attributes;
using Ocow.Auth.Extensions;
using Ocow.Cache.Interfaces;
using Ocow.Jobs.Api.Controllers.Admin;

namespace Ocow.Tests.Unit;

/// <summary>
/// Jobs 服务边界测试，用于验证 Ocow.Jobs 作为独立 Web 项目承载后台任务管理入口。
/// </summary>
public class JobsBoundaryTests
{
    /// <summary>
    /// 验证后台任务 Controller 需要 Admin 授权策略保护。
    /// </summary>
    [Fact]
    public void AdminJobsController_ShouldRequireAdminPolicy()
    {
        var authorize = typeof(AdminJobsController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .OfType<AuthorizeAttribute>()
            .Single();

         Assert.Equal(AuthServiceCollectionExtensions.AdminOnlyPolicy, authorize.Policy);
    }

    /// <summary>
    /// 验证后台测试任务触发入口使用 scheduler.trigger 权限点。
    /// </summary>
    [Fact]
    public void EnqueueSampleAsync_ShouldRequireSchedulerTriggerPermission()
    {
        var method = typeof(AdminJobsController).GetMethod(nameof(AdminJobsController.EnqueueSampleAsync));
        var route = typeof(AdminJobsController)
            .GetCustomAttributes(typeof(RouteAttribute), true)
            .OfType<RouteAttribute>()
            .Single();
        var permission = method!
            .GetCustomAttributes(typeof(PermissionAuthorizeAttribute), true)
            .OfType<PermissionAuthorizeAttribute>()
            .Single();

        Assert.Equal("api/admin/jobs", route.Template);
        Assert.Equal($"{PermissionAuthorizeAttribute.PolicyPrefix}scheduler.trigger", permission.Policy);
    }

    /// <summary>
    /// 验证后台 Dashboard 会话创建入口使用 scheduler.trigger 权限点。
    /// </summary>
    [Fact]
    public void CreateDashboardSessionAsync_ShouldRequireSchedulerTriggerPermission()
    {
        var method = typeof(AdminJobsController).GetMethod(nameof(AdminJobsController.CreateDashboardSessionAsync));
        var httpPost = method!
            .GetCustomAttributes(typeof(HttpPostAttribute), true)
            .OfType<HttpPostAttribute>()
            .Single();
        var permission = method
            .GetCustomAttributes(typeof(PermissionAuthorizeAttribute), true)
            .OfType<PermissionAuthorizeAttribute>()
            .Single();

        Assert.Equal("dashboard-session", httpPost.Template);
        Assert.Equal($"{PermissionAuthorizeAttribute.PolicyPrefix}scheduler.trigger", permission.Policy);
    }

    /// <summary>
    /// 验证 Jobs 服务引用缓存项目，因为 Admin JWT 会话校验依赖统一缓存服务。
    /// </summary>
    [Fact]
    public void JobsApi_ShouldReferenceCacheForAdminTokenSessionValidation()
    {
        var references = typeof(AdminJobsController)
            .Assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name)
            .ToArray();

        Assert.Equal("Ocow.Cache", typeof(ICacheService).Assembly.GetName().Name);
        Assert.Contains("Ocow.Cache", references);
    }
}

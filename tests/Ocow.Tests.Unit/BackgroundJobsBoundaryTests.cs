using System.Security.Claims;
using Microsoft.Extensions.Logging.Abstractions;
using Ocow.BackgroundJobs.Authorization;
using Ocow.BackgroundJobs.Jobs;
using Ocow.BackgroundJobs.Options;

namespace Ocow.Tests.Unit;

/// <summary>
/// BackgroundJobs 边界测试，用于验证 Hangfire 通用封装、后台 Dashboard 鉴权和测试任务示例归属 Ocow.BackgroundJobs。
/// </summary>
public class BackgroundJobsBoundaryTests
{
    /// <summary>
    /// 验证后台任务配置默认 Dashboard 路径和权限点符合后台管理约定。
    /// </summary>
    [Fact]
    public void BackgroundJobsOption_ShouldUseDashboardDefaults()
    {
        var option = new BackgroundJobsOption();

        Assert.Equal("/hangfire", option.DashboardPath);
        Assert.Equal("scheduler.trigger", option.DashboardPermissionCode);
        Assert.Equal("Ocow.HangfireDashboard", option.DashboardCookieName);
        Assert.Equal(30, option.DashboardCookieExpireMinutes);
    }

    /// <summary>
    /// 验证 Hangfire Dashboard 只允许后台管理员访问。
    /// </summary>
    [Fact]
    public void HangfireDashboardAdminAuthorizer_ShouldRequireAdminScope()
    {
        var authorizer = new HangfireDashboardAdminAuthorizer();
        var adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("scope", "admin"),
            new Claim("permission", "scheduler.trigger")
        ], "AdminJwt"));
        var customerPrincipal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("scope", "client"),
            new Claim("permission", "scheduler.trigger")
        ], "CustomerJwt"));
        var adminWithoutPermission = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("scope", "admin")
        ], "AdminJwt"));

        Assert.True(authorizer.IsAuthorized(adminPrincipal));
        Assert.False(authorizer.IsAuthorized(customerPrincipal));
        Assert.False(authorizer.IsAuthorized(adminWithoutPermission));
        Assert.False(authorizer.IsAuthorized(new ClaimsPrincipal()));
    }

    /// <summary>
    /// 验证测试任务示例可以生成可观察的执行结果。
    /// </summary>
    [Fact]
    public async Task SampleBackgroundJob_ShouldReturnExecutionMessage()
    {
        var job = new SampleBackgroundJob(NullLogger<SampleBackgroundJob>.Instance);

        var message = await job.RunAsync(CancellationToken.None);

        Assert.StartsWith("Ocow BackgroundJobs 示例任务已执行：", message);
    }
}

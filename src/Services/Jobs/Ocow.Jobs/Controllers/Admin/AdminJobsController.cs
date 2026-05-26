using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ocow.AspNetCore.Controllers;
using Ocow.Auth.Attributes;
using Ocow.BackgroundJobs.Authorization;
using Ocow.BackgroundJobs.Jobs;
using Ocow.BackgroundJobs.Options;
using Ocow.Jobs.Api.Dtos;
using Ocow.Jobs.Api.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Jobs.Api.Controllers.Admin;

/// <summary>
/// 后台任务接口，用于管理端手动触发后台任务和进入 Hangfire Dashboard。
/// </summary>
[Route("api/admin/jobs")]
[Tags("后台任务")]
public class AdminJobsController : AdminController
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly BackgroundJobsOption _backgroundJobsOption;

    /// <summary>
    /// 创建后台任务 Controller。
    /// </summary>
    public AdminJobsController(
        IBackgroundJobClient backgroundJobClient,
        IOptions<BackgroundJobsOption> backgroundJobsOption)
    {
        _backgroundJobClient = backgroundJobClient;
        _backgroundJobsOption = backgroundJobsOption.Value;
    }

    /// <summary>
    /// 创建 Hangfire Dashboard 浏览器会话，用于让已登录管理员打开 Dashboard 面板。
    /// </summary>
    [HttpPost("dashboard-session")]
    [PermissionAuthorize("scheduler.trigger")]
    public async Task<ApiResDto<DashboardSessionResDto>> CreateDashboardSessionAsync()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(Math.Max(1, _backgroundJobsOption.DashboardCookieExpireMinutes));
        await HttpContext.SignInAsync(
            BackgroundJobsAuthenticationSchemes.DashboardCookie,
            User,
            new AuthenticationProperties
            {
                ExpiresUtc = expiresAt,
                IsPersistent = false
            });

        return Success(new DashboardSessionResDto
        {
            DashboardPath = _backgroundJobsOption.DashboardPath,
            ExpiresAt = expiresAt
        });
    }

    /// <summary>
    /// 手动触发后台任务测试示例。
    /// </summary>
    [HttpPost("sample")]
    [PermissionAuthorize("scheduler.trigger")]
    public Task<ApiResDto<EnqueueJobResDto>> EnqueueSampleAsync()
    {
        var jobId = _backgroundJobClient.Enqueue<SampleBackgroundJob>(job => job.RunAsync(CancellationToken.None));
        var result = new EnqueueJobResDto
        {
            JobId = jobId,
            Name = "sample-background-job",
            DashboardPath = "/hangfire"
        };

        return Task.FromResult(Success(result));
    }


}

using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.Auth.Extensions;
using Ocow.BackgroundJobs.Authorization;
using Ocow.BackgroundJobs.Options;

namespace Ocow.BackgroundJobs.Extensions;

/// <summary>
/// 后台任务应用程序扩展，用于注册 Hangfire Dashboard 中间件。
/// </summary>
public static class BackgroundJobsApplicationBuilderExtensions
{
    /// <summary>
    /// 注册 Ocow Hangfire Dashboard，并使用后台管理员权限保护访问入口。
    /// </summary>
    public static IApplicationBuilder UseOcowHangfireDashboard(this IApplicationBuilder app)
    {
        var option = app.ApplicationServices.GetRequiredService<IOptions<BackgroundJobsOption>>().Value;
        app.UseWhen(
            context => context.Request.Path.StartsWithSegments(option.DashboardPath),
            branch =>
            {
                branch.Use(async (context, next) =>
                {
                    var result = await context.AuthenticateAsync(BackgroundJobsAuthenticationSchemes.DashboardCookie);
                    if (!result.Succeeded)
                    {
                        result = await context.AuthenticateAsync(AuthServiceCollectionExtensions.AdminJwtScheme);
                    }

                    if (result.Succeeded && result.Principal is not null)
                    {
                        context.User = result.Principal;
                    }

                    await next();
                });
            });

        return app.UseHangfireDashboard(option.DashboardPath, new DashboardOptions
        {
            DashboardTitle = option.DashboardTitle,
            Authorization =
            [
                new HangfireDashboardAuthorizationFilter(option.DashboardPermissionCode)
            ]
        });
    }
}

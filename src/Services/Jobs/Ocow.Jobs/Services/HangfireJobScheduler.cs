using Hangfire;
using Ocow.Jobs.Api.Interfaces;
using Ocow.Jobs.Api.Jobs;

namespace Ocow.Jobs.Api.Services;

/// <summary>
/// Hangfire 任务调度器实现，用于注册和触发通用 HTTP 后台任务。
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBackgroundJobClient _backgroundJobClient;

    /// <summary>
    /// 创建 Hangfire 任务调度器。
    /// </summary>
    public HangfireJobScheduler(IRecurringJobManager recurringJobManager, IBackgroundJobClient backgroundJobClient)
    {
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
    }

    /// <summary>
    /// 添加或更新周期任务。
    /// </summary>
    public void AddOrUpdate(Guid id, string cron)
    {
        _recurringJobManager.AddOrUpdate<GenericHttpJob>(
            id.ToString(),
            job => job.RunAsync(id, CancellationToken.None),
            cron);
    }

    /// <summary>
    /// 删除已存在的周期任务。
    /// </summary>
    public void RemoveIfExists(Guid id)
    {
        _recurringJobManager.RemoveIfExists(id.ToString());
    }

    /// <summary>
    /// 手动入队执行一次任务。
    /// </summary>
    public string Enqueue(Guid id)
    {
        return _backgroundJobClient.Enqueue<GenericHttpJob>(job => job.RunAsync(id, CancellationToken.None));
    }
}

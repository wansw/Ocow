namespace Ocow.Jobs.Api.Interfaces;

/// <summary>
/// 任务调度器接口，用于隔离 Hangfire 注册和手动触发能力。
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// 添加或更新周期任务。
    /// </summary>
    void AddOrUpdate(Guid id, string cron);

    /// <summary>
    /// 删除已存在的周期任务。
    /// </summary>
    void RemoveIfExists(Guid id);

    /// <summary>
    /// 手动入队执行一次任务。
    /// </summary>
    string Enqueue(Guid id);
}

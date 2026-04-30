namespace Ocow.EntityFrameworkCore.Seeders;

/// <summary>
/// 数据播种执行器接口，用于按顺序运行一组种子数据初始化任务。
/// </summary>
public interface IDataSeedRunner
{
    /// <summary>
    /// 运行全部种子数据初始化任务。
    /// </summary>
    Task<IReadOnlyList<SeedExecutionResult>> RunAsync(CancellationToken cancellationToken = default);
}

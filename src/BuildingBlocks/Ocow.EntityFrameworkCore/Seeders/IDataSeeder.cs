namespace Ocow.EntityFrameworkCore.Seeders;

/// <summary>
/// 数据播种器接口，用于定义单个种子数据初始化任务。/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// 执行种子数据初始化。    /// </summary>
    Task<SeedExecutionResult> SeedAsync(CancellationToken cancellationToken = default);
}

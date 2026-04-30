namespace Ocow.EntityFrameworkCore.Seeders;

/// <summary>
/// 种子数据执行结果，用于记录本次初始化插入、更新和跳过数量。
/// </summary>
public class SeedExecutionResult
{
    /// <summary>
    /// 播种器名称。
    /// </summary>
    public string SeederName { get; init; } = string.Empty;

    /// <summary>
    /// 新增数据数量。
    /// </summary>
    public int Inserted { get; init; }

    /// <summary>
    /// 更新数据数量。
    /// </summary>
    public int Updated { get; init; }

    /// <summary>
    /// 跳过数据数量。
    /// </summary>
    public int Skipped { get; init; }

    /// <summary>
    /// 执行说明。
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// 创建完成状态的执行结果。
    /// </summary>
    public static SeedExecutionResult Completed(string seederName, int inserted, int updated, int skipped = 0, string message = "")
    {
        return new SeedExecutionResult
        {
            SeederName = seederName,
            Inserted = inserted,
            Updated = updated,
            Skipped = skipped,
            Message = message
        };
    }
}

using Microsoft.Extensions.Logging;

namespace Ocow.BackgroundJobs.Jobs;

/// <summary>
/// 后台任务测试示例，用于验证 Hangfire 任务注册、手动触发和执行日志链路。
/// </summary>
public class SampleBackgroundJob
{
    private readonly ILogger<SampleBackgroundJob> _logger;

    /// <summary>
    /// 创建后台任务测试示例。
    /// </summary>
    public SampleBackgroundJob(ILogger<SampleBackgroundJob> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 执行测试任务并返回可观察的执行消息。
    /// </summary>
    public Task<string> RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var message = $"Ocow BackgroundJobs 示例任务已执行：{DateTimeOffset.UtcNow:O}";
        _logger.LogInformation("{Message}", message);

        return Task.FromResult(message);
    }
}

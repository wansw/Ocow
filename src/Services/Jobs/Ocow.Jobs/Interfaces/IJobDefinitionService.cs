using Ocow.Jobs.Api.Dtos;

namespace Ocow.Jobs.Api.Interfaces;

/// <summary>
/// 动态任务定义服务接口，用于后台页面配置和触发任务。
/// </summary>
public interface IJobDefinitionService
{
    /// <summary>
    /// 创建或更新通用任务配置。
    /// </summary>
    Task<JobDefinitionResDto> AddAsync(CreateJobDefinitionReqDto reqDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// 手动触发一次任务。
    /// </summary>
    Task<TriggerJobResDto> TriggerAsync(Guid id, CancellationToken cancellationToken = default);
}

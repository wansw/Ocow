using Microsoft.EntityFrameworkCore;
using Ocow.Jobs.Api.Data;
using Ocow.Jobs.Api.Dtos;
using Ocow.Jobs.Api.Interfaces;
using Ocow.Jobs.Api.Models;

namespace Ocow.Jobs.Api.Services;

/// <summary>
/// 任务配置服务，用于保存任务定义并同步注册 Hangfire。
/// </summary>
public class JobDefinitionService : IJobDefinitionService
{
    private readonly JobsDbContext _dbContext;
    private readonly IJobScheduler _jobScheduler;

    /// <summary>
    /// 创建任务配置服务。
    /// </summary>
    public JobDefinitionService(JobsDbContext dbContext, IJobScheduler jobScheduler)
    {
        _dbContext = dbContext;
        _jobScheduler = jobScheduler;
    }

    /// <summary>
    /// 创建或更新通用任务配置。
    /// </summary>
    public async Task<JobDefinitionResDto> AddAsync(CreateJobDefinitionReqDto reqDto, CancellationToken cancellationToken = default)
    {

        var definition = new JobDefinition
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        definition.JobName = reqDto.JobName.Trim();
        definition.JobType = reqDto.JobType.Trim();
        definition.Cron = reqDto.Cron.Trim();
        definition.TargetService = reqDto.TargetService.Trim();
        definition.TargetApi = reqDto.TargetApi.Trim();
        definition.HttpMethod = reqDto.HttpMethod.Trim().ToUpperInvariant();
        definition.RequestBody = reqDto.RequestBody;
        definition.Enabled = reqDto.Enabled;
        definition.UpdatedAt = DateTime.UtcNow;

        _dbContext.JobDefinitions.Add(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (definition.Enabled)
        {
            _jobScheduler.AddOrUpdate(definition.Id, definition.Cron);
        }
        else
        {
            _jobScheduler.RemoveIfExists(definition.Id);
        }

        return MapToResDto(definition);
    }

    /// <summary>
    /// 手动触发一次任务。
    /// </summary>
    public async Task<TriggerJobResDto> TriggerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var definition = await _dbContext.JobDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"任务配置不存在：{id}");
        var backgroundJobId = _jobScheduler.Enqueue(definition.Id);

        return new TriggerJobResDto
        {
            id = definition.Id,
            BackgroundJobId = backgroundJobId
        };
    }

    /// <summary>
    /// 将任务配置实体转换为响应 DTO。
    /// </summary>
    private static JobDefinitionResDto MapToResDto(JobDefinition definition)
    {
        return new JobDefinitionResDto
        {
            Id = definition.Id,
            JobName = definition.JobName,
            JobType = definition.JobType,
            Cron = definition.Cron,
            Enabled = definition.Enabled,
            TargetService = definition.TargetService,
            TargetApi = definition.TargetApi,
            HttpMethod = definition.HttpMethod
        };
    }
}

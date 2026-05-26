using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Ocow.AspNetCore.Controllers;
using Ocow.Auth.Attributes;
using Ocow.BackgroundJobs.Options;
using Ocow.Jobs.Api.Dtos;
using Ocow.Jobs.Api.Interfaces;
using Ocow.Shared.Dtos;

namespace Ocow.Jobs.Api.Controllers.Admin
{
    /// <summary>
    /// 自定义任务
    /// </summary>
    [Route("api/admin/JobDefinition")]
    [Tags("自定义任务")]
    public class JobDefinitionController : AdminController
    {
        private readonly IJobDefinitionService _jobDefinitionService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="backgroundJobClient"></param>
        /// <param name="jobDefinitionService"></param>
        /// <param name="backgroundJobsOption"></param>
        public JobDefinitionController(
         IBackgroundJobClient backgroundJobClient,
        IJobDefinitionService jobDefinitionService,
        IOptions<BackgroundJobsOption> backgroundJobsOption)
        {
            _jobDefinitionService = jobDefinitionService;
        }

        /// <summary>
        /// 创建或更新任务配置，并同步注册 Hangfire 周期任务。
        /// </summary>
        [HttpPost("definitions")]
        [PermissionAuthorize("scheduler.trigger")]
        public async Task<ApiResDto<JobDefinitionResDto>> CreateJobDefinitionAsync([FromBody] CreateJobDefinitionReqDto reqDto, CancellationToken cancellationToken)
        {
            var result = await _jobDefinitionService.AddAsync(reqDto, cancellationToken);
            return Success(result);
        }

        /// <summary>
        /// 手动触发一次指定任务。
        /// </summary>
        [HttpPost("definitions/{id}/trigger")]
        [PermissionAuthorize("scheduler.trigger")]
        public async Task<ApiResDto<TriggerJobResDto>> TriggerJobDefinitionAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _jobDefinitionService.TriggerAsync(id, cancellationToken);
            return Success(result);
        }
    }
}



using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ocow.Auth.Attributes;
using Ocow.Jobs.Api.Controllers.Admin;
using Ocow.Jobs.Api.Data;
using Ocow.Jobs.Api.Dtos;
using Ocow.Jobs.Api.Interfaces;
using Ocow.Jobs.Api.Jobs;
using Ocow.Jobs.Api.Models;
using Ocow.Jobs.Api.Options;
using Ocow.Jobs.Api.Services;

namespace Ocow.Tests.Unit;

/// <summary>
/// Jobs 动态任务测试，用于验证 Jobs 只保存通用任务配置和执行日志。
/// </summary>
public class JobsDynamicTaskTests
{
    /// <summary>
    /// 验证创建任务定义接口使用 scheduler.trigger 权限点。
    /// </summary>
    [Fact]
    public void CreateJobDefinitionAsync_ShouldRequireSchedulerTriggerPermission()
    {
        var method = typeof(JobDefinitionController).GetMethod(nameof(JobDefinitionController.CreateJobDefinitionAsync));
        var httpPost = method!
            .GetCustomAttributes(typeof(HttpPostAttribute), true)
            .OfType<HttpPostAttribute>()
            .Single();
        var permission = method
            .GetCustomAttributes(typeof(PermissionAuthorizeAttribute), true)
            .OfType<PermissionAuthorizeAttribute>()
            .Single();

        Assert.Equal("definitions", httpPost.Template);
        Assert.Equal($"{PermissionAuthorizeAttribute.PolicyPrefix}scheduler.trigger", permission.Policy);
    }

    /// <summary>
    /// 验证手动触发任务定义接口使用 scheduler.trigger 权限点。
    /// </summary>
    [Fact]
    public void TriggerJobDefinitionAsync_ShouldRequireSchedulerTriggerPermission()
    {
        var method = typeof(JobDefinitionController).GetMethod(nameof(JobDefinitionController.TriggerJobDefinitionAsync));
        var httpPost = method!
            .GetCustomAttributes(typeof(HttpPostAttribute), true)
            .OfType<HttpPostAttribute>()
            .Single();
        var permission = method
            .GetCustomAttributes(typeof(PermissionAuthorizeAttribute), true)
            .OfType<PermissionAuthorizeAttribute>()
            .Single();

        Assert.Equal("definitions/{id}/trigger", httpPost.Template);
        Assert.Equal($"{PermissionAuthorizeAttribute.PolicyPrefix}scheduler.trigger", permission.Policy);
    }

    /// <summary>
    /// 验证任务配置服务会保存任务定义并注册 Hangfire。
    /// </summary>
    [Fact]
    public async Task JobDefinitionService_AddOrUpdateAsync_ShouldSaveDefinitionAndRegisterRecurringJob()
    {
        await using var dbContext = CreateDbContext();
        var scheduler = new FakeJobScheduler();
        var service = new JobDefinitionService(dbContext, scheduler);
        var reqDto = new CreateJobDefinitionReqDto
        {
            JobName = "通用 HTTP 任务",
            JobType = "Http",
            Cron = "*/5 * * * *",
            TargetService = "RemoteService",
            TargetApi = "https://remote.local/internal/tasks/run",
            RequestBody = "{\"source\":\"demo\"}",
            Enabled = true
        };

        var result = await service.AddAsync(reqDto, CancellationToken.None);

        var definition = await dbContext.JobDefinitions.SingleAsync();
        Assert.Equal("RemoteService", definition.TargetService);
        Assert.Equal("https://remote.local/internal/tasks/run", definition.TargetApi);
        Assert.Equal(definition.Id.ToString(), scheduler.RegisteredJobCode);
        Assert.Equal("*/5 * * * *", scheduler.RegisteredCron);
    }

    /// <summary>
    /// 验证通用 HTTP 任务会读取任务定义、调用目标接口并记录执行日志。
    /// </summary>
    [Fact]
    public async Task GenericHttpJob_RunAsync_ShouldDispatchConfiguredRequestAndWriteExecutionLog()
    {
        await using var dbContext = CreateDbContext();
        var jobMolde = new JobDefinition
        {
            Id = Guid.NewGuid(),
            JobName = "通用 HTTP 任务",
            JobType = "Http",
            Cron = "*/5 * * * *",
            TargetService = "RemoteService",
            TargetApi = "https://remote.local/internal/tasks/run",
            HttpMethod = "POST",
            RequestBody = "{\"source\":\"demo\"}",
            Enabled = true
        };
        dbContext.JobDefinitions.Add(jobMolde);
        await dbContext.SaveChangesAsync();
        var handler = new CaptureHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var job = CreateGenericHttpJob(dbContext, httpClient);

        await job.RunAsync(jobMolde.Id, CancellationToken.None);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request.Method);
        Assert.Equal("https://remote.local/internal/tasks/run", handler.Request.RequestUri?.ToString());
        Assert.Equal("{\"source\":\"demo\"}", handler.RequestBody);
        var log = await dbContext.JobExecutionLogs.SingleAsync();
        Assert.Equal(jobMolde.Id.ToString(), log.JobCode);
        Assert.True(log.Success);
        Assert.Equal(200, log.StatusCode);
    }

    /// <summary>
    /// 验证通用 HTTP 任务会组合目标服务地址和相对接口路径。
    /// </summary>
    [Fact]
    public async Task GenericHttpJob_RunAsync_ShouldCombineTargetServiceAndRelativeTargetApi()
    {
        await using var dbContext = CreateDbContext();
        var jobModel = new JobDefinition
        {
            Id = Guid.NewGuid(),
            JobName = "ERP同步",
            JobType = "Http",
            Cron = "*/1 * * * *",
            TargetService = "https://localhost:7101",
            TargetApi = "/internal/orders/sync/erp",
            HttpMethod = "POST",
            RequestBody = "{}",
            Enabled = true
        };
        dbContext.JobDefinitions.Add(jobModel);
        await dbContext.SaveChangesAsync();
        var handler = new CaptureHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var job = CreateGenericHttpJob(dbContext, httpClient);

        await job.RunAsync(jobModel.Id, CancellationToken.None);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://localhost:7101/internal/orders/sync/erp", handler.Request.RequestUri?.ToString());
        var log = await dbContext.JobExecutionLogs.SingleAsync();
        Assert.True(log.Success);
    }

    /// <summary>
    /// 验证通用 HTTP 任务会按服务名称解析环境内网地址。
    /// </summary>
    [Fact]
    public async Task GenericHttpJob_RunAsync_ShouldResolveBaseUrlByTargetServiceName()
    {
        await using var dbContext = CreateDbContext();
        var jobModel = new JobDefinition
        {
            Id = Guid.NewGuid(),
            JobName = "ERP同步",
            JobType = "Http",
            Cron = "*/1 * * * *",
            TargetService = "ocow-order-api",
            TargetApi = "/internal/orders/sync/erp",
            HttpMethod = "POST",
            RequestBody = "{}",
            Enabled = true
        };
        dbContext.JobDefinitions.Add(jobModel);
        await dbContext.SaveChangesAsync();
        var handler = new CaptureHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var endpointOption = Options.Create(new ServiceEndpointOption
        {
            Services = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ocow-order-api"] = "https://localhost:7101"
            }
        });
        var job = new GenericHttpJob(dbContext, httpClient, endpointOption);

        await job.RunAsync(jobModel.Id, CancellationToken.None);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://localhost:7101/internal/orders/sync/erp", handler.Request.RequestUri?.ToString());
        var log = await dbContext.JobExecutionLogs.SingleAsync();
        Assert.True(log.Success);
    }

    /// <summary>
    /// 验证未配置服务映射时会把目标服务名称当作内网 DNS 主机名。
    /// </summary>
    [Fact]
    public async Task GenericHttpJob_RunAsync_ShouldUseTargetServiceAsDnsHostWhenMappingIsMissing()
    {
        await using var dbContext = CreateDbContext();
        var jobModel = new JobDefinition
        {
            Id = Guid.NewGuid(),
            JobName = "ERP同步",
            JobType = "Http",
            Cron = "*/1 * * * *",
            TargetService = "ocow-order-api",
            TargetApi = "/internal/orders/sync/erp",
            HttpMethod = "POST",
            RequestBody = "{}",
            Enabled = true
        };
        dbContext.JobDefinitions.Add(jobModel);
        await dbContext.SaveChangesAsync();
        var handler = new CaptureHttpMessageHandler();
        using var httpClient = new HttpClient(handler);
        var job = CreateGenericHttpJob(dbContext, httpClient);

        await job.RunAsync(jobModel.Id, CancellationToken.None);

        Assert.NotNull(handler.Request);
        Assert.Equal("http://ocow-order-api/internal/orders/sync/erp", handler.Request.RequestUri?.ToString());
        var log = await dbContext.JobExecutionLogs.SingleAsync();
        Assert.True(log.Success);
    }

    /// <summary>
    /// 创建通用 HTTP 任务测试实例。
    /// </summary>
    private static GenericHttpJob CreateGenericHttpJob(
        JobsDbContext dbContext,
        HttpClient httpClient,
        ServiceEndpointOption? endpointOption = null)
    {
        return new GenericHttpJob(dbContext, httpClient, Options.Create(endpointOption ?? new ServiceEndpointOption()));
    }

    /// <summary>
    /// 验证 Jobs Api 不再定义 Order/ERP 业务类型。
    /// </summary>
    [Fact]
    public void JobsApi_ShouldNotDefineOrderOrErpSpecificTypes()
    {
        var typeNames = typeof(AdminJobsController)
            .Assembly
            .GetTypes()
            .Where(x => x.Namespace?.StartsWith("Ocow.Jobs.Api", StringComparison.Ordinal) == true)
            .Select(x => x.Name)
            .ToArray();

        Assert.DoesNotContain(typeNames, x => x.Contains("Order", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeNames, x => x.Contains("Erp", StringComparison.OrdinalIgnoreCase));
    }

    private static JobsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<JobsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new JobsDbContext(options);
    }

    private sealed class FakeJobScheduler : IJobScheduler
    {
        public string? RegisteredJobCode { get; private set; }

        public string? RegisteredCron { get; private set; }

        /// <summary>
        /// 记录测试任务注册参数。
        /// </summary>
        public void AddOrUpdate(Guid id, string cron)
        {
            RegisteredJobCode = id.ToString();
            RegisteredCron = cron;
        }

        /// <summary>
        /// 测试场景不需要删除任务。
        /// </summary>
        public void RemoveIfExists(Guid id)
        {
        }

        /// <summary>
        /// 测试场景不需要触发任务。
        /// </summary>
        public string Enqueue(Guid id)
        {
            return $"manual-{id.ToString()}";
        }
    }

    private sealed class CaptureHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public string? RequestBody { get; private set; }

        /// <summary>
        /// 捕获通用 HTTP 任务请求并返回成功响应。
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"success\":true}", Encoding.UTF8, "application/json")
            };
        }
    }
}

using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ocow.Jobs.Api.Data;
using Ocow.Jobs.Api.Models;
using Ocow.Jobs.Api.Options;

namespace Ocow.Jobs.Api.Jobs;

/// <summary>
/// 通用 HTTP 后台任务，用于读取任务配置并调用目标接口。
/// </summary>
public class GenericHttpJob
{
    private readonly JobsDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ServiceEndpointOption _serviceEndpointOption;

    /// <summary>
    /// 创建通用 HTTP 后台任务。
    /// </summary>
    public GenericHttpJob(
        JobsDbContext dbContext,
        HttpClient httpClient,
        IOptions<ServiceEndpointOption> serviceEndpointOption)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _serviceEndpointOption = serviceEndpointOption.Value;
    }

    /// <summary>
    /// 按任务编码执行通用 HTTP 请求并记录执行日志。
    /// </summary>
    public async Task RunAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var definition = await _dbContext.JobDefinitions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"任务配置不存在：{id}");

        if (!definition.Enabled)
        {
            return;
        }

        var log = new JobExecutionLog
        {
            Id = Guid.NewGuid(),
            JobDefinitionId = definition.Id,
            JobCode = definition.Id.ToString(),
            StartedAt = DateTime.UtcNow
        };

        try
        {
            using var request = CreateRequest(definition);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            log.StatusCode = (int)response.StatusCode;
            log.ResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            log.Success = response.IsSuccessStatusCode;
            log.Message = response.IsSuccessStatusCode ? "success" : response.ReasonPhrase;
        }
        catch (Exception ex)
        {
            log.Success = false;
            log.Message = ex.Message;
        }
        finally
        {
            log.EndedAt = DateTime.UtcNow;
            _dbContext.JobExecutionLogs.Add(log);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// 根据任务配置创建 HTTP 请求。
    /// </summary>
    private HttpRequestMessage CreateRequest(JobDefinition definition)
    {
        var request = new HttpRequestMessage(new HttpMethod(definition.HttpMethod), BuildRequestUri(definition));
        if (!string.IsNullOrWhiteSpace(definition.RequestBody) &&
            !string.Equals(definition.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
        {
            request.Content = new StringContent(definition.RequestBody, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    /// 组合任务配置中的服务地址和接口路径，生成可直接请求的绝对 URI。
    /// </summary>
    private Uri BuildRequestUri(JobDefinition definition)
    {
        var targetApi = definition.TargetApi.Trim();
        if (Uri.TryCreate(targetApi, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        var serviceBaseUrl = ResolveServiceBaseUrl(definition.TargetService);
        if (!Uri.TryCreate(serviceBaseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new InvalidOperationException($"目标服务地址配置无效：{definition.TargetService}");
        }

        if (!Uri.TryCreate(baseUri, targetApi, out var requestUri))
        {
            throw new InvalidOperationException("任务目标接口地址配置无效。");
        }

        return requestUri;
    }

    /// <summary>
    /// 根据任务配置中的服务名称解析当前环境可访问的服务基础地址。
    /// </summary>
    private string ResolveServiceBaseUrl(string targetService)
    {
        var serviceName = targetService.Trim();
        if (Uri.TryCreate(serviceName, UriKind.Absolute, out _))
        {
            return serviceName;
        }

        var service = _serviceEndpointOption.Services
            .FirstOrDefault(x => string.Equals(x.Key, serviceName, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(service.Value))
        {
            return service.Value.Trim();
        }

        return $"http://{serviceName}";
    }
}

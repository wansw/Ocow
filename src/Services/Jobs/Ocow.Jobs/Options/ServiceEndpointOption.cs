namespace Ocow.Jobs.Api.Options;

/// <summary>
/// 服务地址配置，用于把任务配置中的逻辑服务名称解析为当前环境可访问的内网地址。
/// </summary>
public class ServiceEndpointOption
{
    /// <summary>
    /// 配置节点名称。
    /// </summary>
    public const string SectionName = "ServiceEndpoints";

    /// <summary>
    /// 服务名称和服务基础地址映射。
    /// </summary>
    public Dictionary<string, string> Services { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

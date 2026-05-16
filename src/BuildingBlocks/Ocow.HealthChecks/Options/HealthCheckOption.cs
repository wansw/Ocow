namespace Ocow.HealthChecks.Options;

/// <summary>
/// 健康检查配置项，用于设置服务名称和健康检查端点路径。
/// </summary>
public class HealthCheckOption
{
    /// <summary>
    /// 服务名称。
    /// </summary>
    public string ServiceName { get; set; } = "Ocow.Api";

    /// <summary>
    /// 完整健康检查路径。
    /// </summary>
    public string HealthPath { get; set; } = "/health";

    /// <summary>
    /// 存活检查路径。
    /// </summary>
    public string LivePath { get; set; } = "/live";

    /// <summary>
    /// 就绪检查路径。
    /// </summary>
    public string ReadyPath { get; set; } = "/ready";
}

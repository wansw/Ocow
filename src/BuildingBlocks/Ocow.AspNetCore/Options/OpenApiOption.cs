namespace Ocow.AspNetCore.Options;

/// <summary>
/// OpenAPI 配置项，用于统一设置服务文档名称、版本和描述。
/// </summary>
public class OpenApiOption
{
    /// <summary>
    /// 服务名称。
    /// </summary>
    public string ServiceName { get; init; } = "Ocow.Api";

    /// <summary>
    /// 文档版本号。
    /// </summary>
    public string Version { get; init; } = "v1";

    /// <summary>
    /// 文档描述。
    /// </summary>
    public string Description { get; init; } = "Ocow Web API";
}

namespace Ocow.Shared.Options;

/// <summary>
/// OpenAPI 配置实体，用于绑定服务名称、版本和说明。/// </summary>
public class OpenApiOption
{
    /// <summary>
    /// 当前服务名称。    /// </summary>
    public string ServiceName { get; init; } = "Ocow.Api";

    /// <summary>
    /// OpenAPI 文档版本号。    /// </summary>
    public string Version { get; init; } = "v1";

    /// <summary>
    /// OpenAPI 文档说明。    /// </summary>
    public string Description { get; init; } = "Ocow REST API";
}

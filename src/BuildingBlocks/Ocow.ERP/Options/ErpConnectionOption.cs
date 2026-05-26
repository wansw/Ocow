namespace Ocow.ERP.Options;

/// <summary>
/// ERP 连接配置实体，用于传递某个同步配置对应的外部 ERP 连接信息。
/// </summary>
public class ErpConnectionOption
{
    /// <summary>
    /// 同步配置编号。
    /// </summary>
    public string? SyncConfigId { get; init; }

    /// <summary>
    /// ERP 编码。
    /// </summary>
    public string ErpCode { get; init; } = string.Empty;
}

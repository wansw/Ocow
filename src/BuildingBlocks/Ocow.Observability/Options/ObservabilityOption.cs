namespace Ocow.Observability.Options;

/// <summary>
/// 可观测性配置项，用于配置结构化日志和链路追踪的基础属性。
/// </summary>
public class ObservabilityOption
{
    /// <summary>
    /// 当前服务名称，用于写入每条结构化日志。
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// 当前服务版本。
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";

    /// <summary>
    /// OpenTelemetry Collector 端点地址。
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// 日志配置。
    /// </summary>
    public LoggingOption Logging { get; set; } = new();

    /// <summary>
    /// 是否启用请求 TraceId 透传。
    /// </summary>
    public bool EnableRequestTrace { get; set; } = true;

    /// <summary>
    /// 是否启用链路追踪。
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// 是否启用指标监控。
    /// </summary>
    public bool EnableMetrics { get; set; } = true;
}

/// <summary>
/// 日志配置项。
/// </summary>
public class LoggingOption
{
    /// <summary>
    /// 最小日志级别。
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// 是否启用控制台输出。
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// 是否启用文件输出。
    /// </summary>
    public bool EnableFile { get; set; } = false;

    /// <summary>
    /// 是否启用 OpenTelemetry 输出。
    /// </summary>
    public bool EnableOpenTelemetry { get; set; } = false;

    /// <summary>
    /// 是否启用请求日志。
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// 文件输出路径。
    /// </summary>
    public string FilePath { get; set; } = "logs/log-.json";

    /// <summary>
    /// 日志文件滚动间隔。
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// 日志保留天数。
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 30;
}

namespace Ocow.EventBus.RabbitMq.Options;

/// <summary>
/// CAP RabbitMQ 事件总线配置项，用于绑定 EventBus 配置节点。
/// </summary>
public sealed class CapRabbitMqEventBusOptions
{
    /// <summary>
    /// CAP 默认消费者分组名称，每个服务必须保持唯一。
    /// </summary>
    public string DefaultGroupName { get; set; } = string.Empty;

    /// <summary>
    /// 失败消息重试次数。
    /// </summary>
    public int FailedRetryCount { get; set; } = 5;

    /// <summary>
    /// 失败消息重试间隔秒数。
    /// </summary>
    public int FailedRetryIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// RabbitMQ transport 配置。
    /// </summary>
    public RabbitMqTransportOptions RabbitMq { get; set; } = new();

    /// <summary>
    /// CAP 存储配置。
    /// </summary>
    public CapStorageOptions Storage { get; set; } = new();

    /// <summary>
    /// CAP Dashboard 配置。
    /// </summary>
    public CapDashboardOptions Dashboard { get; set; } = new();
}

/// <summary>
/// RabbitMQ transport 配置项，用于配置 CAP RabbitMQ 连接信息。
/// </summary>
public sealed class RabbitMqTransportOptions
{
    /// <summary>
    /// RabbitMQ 主机名。
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ 端口。
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ 用户名。
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ 密码。
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// RabbitMQ 虚拟主机。
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// RabbitMQ 交换机名称。
    /// </summary>
    public string ExchangeName { get; set; } = "ocow.events";
}

/// <summary>
/// CAP 存储配置项，用于选择本地消息表数据库 Provider。
/// </summary>
public sealed class CapStorageOptions
{
    /// <summary>
    /// CAP 存储 Provider，支持 SqlServer、PostgreSql、MySql。
    /// </summary>
    public string Provider { get; set; } = "PostgreSql";

    /// <summary>
    /// CAP 存储连接字符串名称，对应 ConnectionStrings 节点。
    /// </summary>
    public string ConnectionStringName { get; set; } = "Default";
}

/// <summary>
/// CAP Dashboard 配置项，用于控制监控页面访问方式。
/// </summary>
public sealed class CapDashboardOptions
{
    /// <summary>
    /// 是否启用 CAP Dashboard。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// CAP Dashboard 路径。
    /// </summary>
    public string PathMatch { get; set; } = "/cap";

    /// <summary>
    /// 是否显式允许匿名访问 CAP Dashboard。
    /// </summary>
    public bool AllowAnonymousExplicit { get; set; }

    /// <summary>
    /// CAP Dashboard 授权策略名称。
    /// </summary>
    public string? AuthorizationPolicy { get; set; }
}

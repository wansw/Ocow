namespace Ocow.HealthChecks.Options;

/// <summary>
/// RabbitMQ 健康检查配置项，用于读取连接地址、账号和虚拟主机。
/// </summary>
public class RabbitMqHealthCheckOption
{
    /// <summary>
    /// RabbitMQ 主机名。
    /// </summary>
    public string HostName { get; init; } = string.Empty;

    /// <summary>
    /// RabbitMQ 端口。
    /// </summary>
    public int Port { get; init; } = 5672;

    /// <summary>
    /// RabbitMQ 用户名。
    /// </summary>
    public string UserName { get; init; } = "admin";

    /// <summary>
    /// RabbitMQ 密码。
    /// </summary>
    public string Password { get; init; } = "admin123";

    /// <summary>
    /// RabbitMQ 虚拟主机。
    /// </summary>
    public string VirtualHost { get; init; } = "/";
}

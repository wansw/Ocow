namespace Ocow.MessageBus.Options;

/// <summary>
/// RabbitMQ 配置实体，用于绑定连接地址、账号和交换机。
/// </summary>
public class RabbitMqOption
{
    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "admin";

    public string Password { get; init; } = "admin123";

    public string ExchangeName { get; init; } = "ocow.events";
}

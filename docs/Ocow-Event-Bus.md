Ocow.MessageBus改名并且拆分成
Ocow.EventBus.Abstractions
Ocow.EventBus.RabbitMq

下面这套方案按你的要求来设计：

``` text
Ocow.Contracts
Ocow.EventBus.Abstractions
Ocow.EventBus.RabbitMq
```

但要明确一点：**`Ocow.EventBus.RabbitMq` 内部不是直接封装 `RabbitMQ.Client`，而是用 CAP + RabbitMQ 实现事件总线**。RabbitMQ 只作为 CAP 的 transport，数据库作为 CAP 的 Outbox / Inbox 存储。CAP 官方定位就是分布式事务和事件总线集成方案，核心是本地消息表 + Outbox，用来解决单纯 MQ 无法保证可靠性的问题。([GitHub](https://github.com/dotnetcore/CAP?utm_source=chatgpt.com "GitHub \- dotnetcore/CAP: Distributed transaction solution in micro ..."))

---

# 0. 先说明“强一致”的边界

你说的“数据库事务和事件发布强一致”，在 CAP 方案里应该理解为：

``` text
业务数据写入数据库
+
事件写入 CAP Outbox 本地消息表

这两件事在同一个数据库事务里提交或回滚。
```

这部分可以做到**本地事务强一致**。

但跨服务消费仍然是异步的，所以整体是：

``` text
本服务内：业务数据 + Outbox 消息强一致
跨服务间：最终一致
```

也就是说，CAP 保证的是“业务数据保存成功时，事件不会丢”，然后 CAP 后台把 Outbox 消息投递到 RabbitMQ，并负责失败重试、状态记录和 Dashboard 查看。CAP 文档也说明它需要同时配置 storage 和 transport，并且会把消息存储起来以保证可靠性和最终一致性。([CAP](https://cap.dotnetcore.xyz/user-guide/en/cap/configuration/?utm_source=chatgpt.com "Configuration \- CAP"))

---

# 1. 最终推荐结构

``` text
src/BuildingBlocks
├─ Ocow.Contracts
│  ├─ Abstractions
│  │  ├─ IntegrationEvent.cs
│  │  └─ IntegrationEventNameAttribute.cs
│  └─ Events
│     └─ Orders
│        └─ OrderCreatedIntegrationEvent.cs
│
├─ Ocow.EventBus.Abstractions
│  ├─ IEventBus.cs
│  ├─ IIntegrationEventHandler.cs
│  ├─ IIntegrationEventNameProvider.cs
│  └─ DefaultIntegrationEventNameProvider.cs
│
└─ Ocow.EventBus.RabbitMq
   ├─ CapRabbitMqEventBus.cs
   ├─ CapRabbitMqEventBusOptions.cs
   ├─ CapRabbitMqEventBusServiceCollectionExtensions.cs
   ├─ ICapTransactionalExecutor.cs
   ├─ CapTransactionalExecutor.cs
   └─ IntegrationEventHandlerRegistrationExtensions.cs
```

这里有一个关键调整：

**`IntegrationEventNameAttribute` 建议放在 `Ocow.Contracts`，不要放在 `Ocow.EventBus.Abstractions`。**

原因是 `Ocow.Contracts` 里的事件类需要标注：

``` csharp
[IntegrationEventName("ocow.orders.created")]
```

如果 attribute 放在 `Ocow.EventBus.Abstractions`，而 `Ocow.EventBus.Abstractions` 又引用 `Ocow.Contracts`，就会形成循环依赖。

正确依赖关系应该是：

``` text
Ocow.Contracts
  不依赖任何 EventBus 项目

Ocow.EventBus.Abstractions
  依赖 Ocow.Contracts

Ocow.EventBus.RabbitMq
  依赖 Ocow.Contracts
  依赖 Ocow.EventBus.Abstractions
  依赖 DotNetCore.CAP
  依赖 DotNetCore.CAP.RabbitMQ
  依赖 DotNetCore.CAP.SqlServer/PostgreSql/MySql
```

---

# 2. NuGet 包建议

## 2.1 `Ocow.Contracts.csproj`

不需要 CAP，不需要 RabbitMQ。

``` xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

---

## 2.2 `Ocow.EventBus.Abstractions.csproj`

``` xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Ocow.Contracts\Ocow.Contracts.csproj" />
  </ItemGroup>

</Project>
```

---

## 2.3 `Ocow.EventBus.RabbitMq.csproj`

CAP 包版本要保持一致。NuGet 页面显示 `DotNetCore.CAP.RabbitMQ` 有 `8.3.2` 版本；实际项目里建议你统一锁定同一个 CAP 版本，比如全部用 `8.3.2`，或者按你解决方案当前 .NET 版本统一升级。([NuGet](https://www.nuget.org/packages/DotNetCore.CAP.RabbitMQ/8.3.2?utm_source=chatgpt.com "NuGet Gallery \| DotNetCore.CAP.RabbitMQ 8.3.2"))

``` xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Ocow.Contracts\Ocow.Contracts.csproj" />
    <ProjectReference Include="..\Ocow.EventBus.Abstractions\Ocow.EventBus.Abstractions.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="DotNetCore.CAP" Version="8.3.2" />
    <PackageReference Include="DotNetCore.CAP.RabbitMQ" Version="8.3.2" />
    <PackageReference Include="DotNetCore.CAP.Dashboard" Version="8.3.2" />

    <PackageReference Include="DotNetCore.CAP.SqlServer" Version="8.3.2" />
    <PackageReference Include="DotNetCore.CAP.PostgreSql" Version="8.3.2" />
    <PackageReference Include="DotNetCore.CAP.MySql" Version="8.3.2" />

    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="8.0.0" />
  </ItemGroup>

</Project>
```

如果你确定只用 SQL Server，可以只保留：

``` xml
<PackageReference Include="DotNetCore.CAP.SqlServer" Version="8.3.2" />
```

如果只用 PostgreSQL，就只保留：

``` xml
<PackageReference Include="DotNetCore.CAP.PostgreSql" Version="8.3.2" />
```

---

# 3. `Ocow.Contracts`

## 3.1 `Abstractions/IntegrationEvent.cs`

``` csharp
namespace Ocow.Contracts.Abstractions;

public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;

    public string? CorrelationId { get; init; }

    public string? CausationId { get; init; }
}
```

说明：

``` text
Id               用于幂等、追踪、排查。
OccurredOnUtc    事件发生时间。
CorrelationId    一条业务链路的关联 ID。
CausationId      触发当前事件的上游事件或命令 ID。
```

---

## 3.2 `Abstractions/IntegrationEventNameAttribute.cs`

``` csharp
namespace Ocow.Contracts.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IntegrationEventNameAttribute : Attribute
{
    public IntegrationEventNameAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Integration event name cannot be empty.", nameof(name));
        }

        Name = name;
    }

    public string Name { get; }
}
```

事件名上线后不要随便改。事件名就是跨服务协议的一部分。

---

## 3.3 `Events/Orders/OrderCreatedIntegrationEvent.cs`

``` csharp
using Ocow.Contracts.Abstractions;

namespace Ocow.Contracts.Events.Orders;

[IntegrationEventName("ocow.orders.created")]
public sealed record OrderCreatedIntegrationEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    string Currency
) : IntegrationEvent;
```

事件命名建议：

``` text
ocow.{业务域}.{事件名}
```

例如：

``` text
ocow.orders.created
ocow.orders.cancelled
ocow.payments.succeeded
ocow.users.registered
```

---

# 4. `Ocow.EventBus.Abstractions`

## 4.1 `IEventBus.cs`

``` csharp
using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions;

public interface IEventBus
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;

    Task PublishAsync<TEvent>(
        string eventName,
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
```

业务代码主要用第一个方法：

``` csharp
await _eventBus.PublishAsync(new OrderCreatedIntegrationEvent(...), cancellationToken);
```

第二个方法用于兼容特殊场景，比如临时指定事件名或灰度版本：

``` csharp
await _eventBus.PublishAsync("ocow.orders.created.v2", integrationEvent, cancellationToken);
```

---

## 4.2 `IIntegrationEventHandler.cs`

``` csharp
using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions;

public interface IIntegrationEventHandler<in TEvent>
    where TEvent : IntegrationEvent
{
    Task HandleAsync(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default);
}
```

业务处理器实现这个接口，不直接依赖 CAP。

---

## 4.3 `IIntegrationEventNameProvider.cs`

``` csharp
using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions;

public interface IIntegrationEventNameProvider
{
    string GetName<TEvent>()
        where TEvent : IntegrationEvent;

    string GetName(Type eventType);
}
```

---

## 4.4 `DefaultIntegrationEventNameProvider.cs`

``` csharp
using System.Reflection;
using System.Text;
using Ocow.Contracts.Abstractions;

namespace Ocow.EventBus.Abstractions;

public sealed class DefaultIntegrationEventNameProvider : IIntegrationEventNameProvider
{
    public string GetName<TEvent>()
        where TEvent : IntegrationEvent
    {
        return GetName(typeof(TEvent));
    }

    public string GetName(Type eventType)
    {
        ArgumentNullException.ThrowIfNull(eventType);

        if (!typeof(IntegrationEvent).IsAssignableFrom(eventType))
        {
            throw new ArgumentException(
                $"Type '{eventType.FullName}' must inherit from IntegrationEvent.",
                nameof(eventType));
        }

        var attribute = eventType.GetCustomAttribute<IntegrationEventNameAttribute>();

        if (attribute is not null)
        {
            return attribute.Name;
        }

        var name = eventType.Name;

        if (name.EndsWith("IntegrationEvent", StringComparison.Ordinal))
        {
            name = name[..^"IntegrationEvent".Length];
        }

        return $"ocow.{ToDotCase(name)}";
    }

    private static string ToDotCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder();

        for (var i = 0; i < value.Length; i++)
        {
            var current = value[i];

            if (char.IsUpper(current))
            {
                if (i > 0)
                {
                    builder.Append('.');
                }

                builder.Append(char.ToLowerInvariant(current));
            }
            else
            {
                builder.Append(current);
            }
        }

        return builder.ToString();
    }
}
```

生产环境建议所有事件都显式加：

``` csharp
[IntegrationEventName("ocow.orders.created")]
```

自动推导只作为 fallback，不要作为主要生产策略。

---

# 5. `Ocow.EventBus.RabbitMq`

这个项目虽然叫 `RabbitMq`，但内部使用 CAP。

也就是说：

``` text
Ocow.EventBus.RabbitMq
  = CAP implementation with RabbitMQ transport
```

不建议在这个项目里直接使用 `RabbitMQ.Client`。CAP 官方支持 RabbitMQ transport，可以通过 `UseRabbitMQ(...)` 配置 RabbitMQ；同时需要配置一个 storage，比如 SQL Server、PostgreSQL、MySQL。([CAP](https://cap.dotnetcore.xyz/user-guide/zh/transport/rabbitmq/?utm_source=chatgpt.com "RabbitMQ \- CAP"))

---

## 5.1 `CapRabbitMqEventBusOptions.cs`

``` csharp
namespace Ocow.EventBus.RabbitMq;

public sealed class CapRabbitMqEventBusOptions
{
    public string DefaultGroupName { get; set; } = default!;

    public int FailedRetryCount { get; set; } = 5;

    public int FailedRetryIntervalSeconds { get; set; } = 60;

    public RabbitMqTransportOptions RabbitMq { get; set; } = new();

    public CapStorageOptions Storage { get; set; } = new();

    public CapDashboardOptions Dashboard { get; set; } = new();
}

public sealed class RabbitMqTransportOptions
{
    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = "guest";

    public string Password { get; set; } = "guest";

    public string VirtualHost { get; set; } = "/";

    public string ExchangeName { get; set; } = "ocow.events";
}

public sealed class CapStorageOptions
{
    public string Provider { get; set; } = "SqlServer";

    public string ConnectionStringName { get; set; } = "Default";
}

public sealed class CapDashboardOptions
{
    public bool Enabled { get; set; } = true;

    public string PathMatch { get; set; } = "/cap";

    public bool AllowAnonymousExplicit { get; set; } = false;

    public string? AuthorizationPolicy { get; set; }
}
```

---

## 5.2 `CapRabbitMqEventBus.cs`

``` csharp
using DotNetCore.CAP;
using Ocow.Contracts.Abstractions;
using Ocow.EventBus.Abstractions;

namespace Ocow.EventBus.RabbitMq;

public sealed class CapRabbitMqEventBus : IEventBus
{
    private readonly ICapPublisher _capPublisher;
    private readonly IIntegrationEventNameProvider _eventNameProvider;

    public CapRabbitMqEventBus(
        ICapPublisher capPublisher,
        IIntegrationEventNameProvider eventNameProvider)
    {
        _capPublisher = capPublisher;
        _eventNameProvider = eventNameProvider;
    }

    public Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var eventName = _eventNameProvider.GetName<TEvent>();

        return PublishAsync(eventName, integrationEvent, cancellationToken);
    }

    public Task PublishAsync<TEvent>(
        string eventName,
        TEvent integrationEvent,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        if (string.IsNullOrWhiteSpace(eventName))
        {
            throw new ArgumentException("Event name cannot be empty.", nameof(eventName));
        }

        ArgumentNullException.ThrowIfNull(integrationEvent);

        var headers = new Dictionary<string, string?>
        {
            ["x-event-id"] = integrationEvent.Id.ToString("N"),
            ["x-event-name"] = eventName,
            ["x-event-type"] = integrationEvent.GetType().FullName,
            ["x-occurred-on-utc"] = integrationEvent.OccurredOnUtc.ToString("O"),
            ["x-correlation-id"] = integrationEvent.CorrelationId,
            ["x-causation-id"] = integrationEvent.CausationId
        };

        return _capPublisher.PublishAsync(
            eventName,
            integrationEvent,
            headers!,
            cancellationToken);
    }
}
```

如果 Codex 编译时提示 `PublishAsync` 参数顺序与你当前 CAP 版本不一致，让它按当前 CAP 包的 `ICapPublisher.PublishAsync` 重载调整为下面这种形式：

``` csharp
return _capPublisher.PublishAsync(
    name: eventName,
    contentObj: integrationEvent,
    headers: headers!,
    cancellationToken: cancellationToken);
```

不同 CAP 小版本的重载签名可能略有差异，核心逻辑不变。

---

## 5.3 `ICapTransactionalExecutor.cs`

这个接口用于解决你要求的：

``` text
数据库事务 + 事件发布强一致
```

它放在 `Ocow.EventBus.RabbitMq`，因为它依赖 EF Core + CAP，不应该放到纯抽象项目里。

``` csharp
using Microsoft.EntityFrameworkCore;
using Ocow.EventBus.Abstractions;

namespace Ocow.EventBus.RabbitMq;

public interface ICapTransactionalExecutor<TDbContext>
    where TDbContext : DbContext
{
    Task ExecuteAsync(
        Func<TDbContext, IEventBus, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task<TResult> ExecuteAsync<TResult>(
        Func<TDbContext, IEventBus, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
```

---

## 5.4 `CapTransactionalExecutor.cs`

这是关键文件。业务代码必须通过它把：

``` text
DbContext 数据变更
+
_eventBus.PublishAsync(...)
```

包进同一个 CAP 事务。

``` csharp
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocow.EventBus.Abstractions;

namespace Ocow.EventBus.RabbitMq;

public sealed class CapTransactionalExecutor<TDbContext>
    : ICapTransactionalExecutor<TDbContext>
    where TDbContext : DbContext
{
    private readonly TDbContext _dbContext;
    private readonly ICapPublisher _capPublisher;
    private readonly IEventBus _eventBus;
    private readonly ILogger<CapTransactionalExecutor<TDbContext>> _logger;

    public CapTransactionalExecutor(
        TDbContext dbContext,
        ICapPublisher capPublisher,
        IEventBus eventBus,
        ILogger<CapTransactionalExecutor<TDbContext>> logger)
    {
        _dbContext = dbContext;
        _capPublisher = capPublisher;
        _eventBus = eventBus;
        _logger = logger;
    }

    public Task ExecuteAsync(
        Func<TDbContext, IEventBus, CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<object?>(
            async (dbContext, eventBus, ct) =>
            {
                await operation(dbContext, eventBus, ct);
                return null;
            },
            cancellationToken);
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        Func<TDbContext, IEventBus, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                _capPublisher,
                autoCommit: false,
                cancellationToken);

            try
            {
                var result = await operation(_dbContext, _eventBus, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to execute CAP transactional operation for DbContext {DbContextType}.",
                    typeof(TDbContext).FullName);

                await transaction.RollbackAsync(cancellationToken);

                throw;
            }
        });
    }
}
```

这个文件依赖 CAP 的 EF Core 事务扩展：

``` csharp
_dbContext.Database.BeginTransactionAsync(_capPublisher, autoCommit: false, cancellationToken)
```

CAP 8.1 以后重新支持异步开启事务发送消息 API；官方发布说明里也给出了 `BeginTransactionAsync(_capBus, true)` 这类用法。([cnblogs.com](https://www.cnblogs.com/savorboard/p/18139824/cap-8-1?utm_source=chatgpt.com "CAP 8.1 版本发布通告 \- Savorboard \- 博客园"))

---

## 5.5 `IntegrationEventHandlerRegistrationExtensions.cs`

``` csharp
using Microsoft.Extensions.DependencyInjection;
using Ocow.Contracts.Abstractions;
using Ocow.EventBus.Abstractions;

namespace Ocow.EventBus.RabbitMq;

public static class IntegrationEventHandlerRegistrationExtensions
{
    public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(
        this IServiceCollection services)
        where TEvent : IntegrationEvent
        where THandler : class, IIntegrationEventHandler<TEvent>
    {
        services.AddScoped<IIntegrationEventHandler<TEvent>, THandler>();

        return services;
    }
}
```

这个只注册业务 Handler。

CAP 订阅仍然建议用一个 Subscriber Adapter 显式写 `[CapSubscribe]`，因为 CAP 的订阅名必须稳定、清晰，第一版不要做自动扫描和动态订阅。

---

## 5.6 `CapRabbitMqEventBusServiceCollectionExtensions.cs`

``` csharp
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocow.EventBus.Abstractions;

namespace Ocow.EventBus.RabbitMq;

public static class CapRabbitMqEventBusServiceCollectionExtensions
{
    public static IServiceCollection AddOcowCapRabbitMqEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "EventBus")
    {
        var section = configuration.GetSection(sectionName);

        services
            .AddOptions<CapRabbitMqEventBusOptions>()
            .Bind(section)
            .Validate(ValidateOptions, "Invalid CAP RabbitMQ event bus configuration.")
            .ValidateOnStart();

        var options = section.Get<CapRabbitMqEventBusOptions>()
            ?? throw new InvalidOperationException($"Missing configuration section: {sectionName}");

        services.AddSingleton<IIntegrationEventNameProvider, DefaultIntegrationEventNameProvider>();
        services.AddScoped<IEventBus, CapRabbitMqEventBus>();
        services.AddScoped(typeof(ICapTransactionalExecutor<>), typeof(CapTransactionalExecutor<>));

        services.AddCap(cap =>
        {
            cap.DefaultGroupName = options.DefaultGroupName;
            cap.FailedRetryCount = options.FailedRetryCount;
            cap.FailedRetryInterval = options.FailedRetryIntervalSeconds;

            ConfigureStorage(cap, configuration, options.Storage);

            cap.UseRabbitMQ(rabbit =>
            {
                rabbit.HostName = options.RabbitMq.HostName;
                rabbit.Port = options.RabbitMq.Port;
                rabbit.UserName = options.RabbitMq.UserName;
                rabbit.Password = options.RabbitMq.Password;
                rabbit.VirtualHost = options.RabbitMq.VirtualHost;
                rabbit.ExchangeName = options.RabbitMq.ExchangeName;
            });

            if (options.Dashboard.Enabled)
            {
                cap.UseDashboard(dashboard =>
                {
                    dashboard.PathMatch = options.Dashboard.PathMatch;
                    dashboard.AllowAnonymousExplicit = options.Dashboard.AllowAnonymousExplicit;

                    if (!string.IsNullOrWhiteSpace(options.Dashboard.AuthorizationPolicy))
                    {
                        dashboard.AuthorizationPolicy = options.Dashboard.AuthorizationPolicy;
                    }
                });
            }
        });

        return services;
    }

    private static void ConfigureStorage(
        CapOptions cap,
        IConfiguration configuration,
        CapStorageOptions storageOptions)
    {
        if (string.IsNullOrWhiteSpace(storageOptions.Provider))
        {
            throw new InvalidOperationException("EventBus:Storage:Provider is required.");
        }

        if (string.IsNullOrWhiteSpace(storageOptions.ConnectionStringName))
        {
            throw new InvalidOperationException("EventBus:Storage:ConnectionStringName is required.");
        }

        var connectionString = configuration.GetConnectionString(storageOptions.ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{storageOptions.ConnectionStringName}' is required.");
        }

        switch (storageOptions.Provider.Trim().ToLowerInvariant())
        {
            case "sqlserver":
                cap.UseSqlServer(connectionString);
                break;

            case "postgresql":
            case "postgres":
                cap.UsePostgreSql(connectionString);
                break;

            case "mysql":
                cap.UseMySql(connectionString);
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported CAP storage provider: {storageOptions.Provider}");
        }
    }

    private static bool ValidateOptions(CapRabbitMqEventBusOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DefaultGroupName))
        {
            return false;
        }

        if (options.FailedRetryCount < 0)
        {
            return false;
        }

        if (options.FailedRetryIntervalSeconds <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.RabbitMq.HostName))
        {
            return false;
        }

        if (options.RabbitMq.Port <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.RabbitMq.UserName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.RabbitMq.Password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.RabbitMq.VirtualHost))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.RabbitMq.ExchangeName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.Storage.Provider))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(options.Storage.ConnectionStringName))
        {
            return false;
        }

        if (options.Dashboard.Enabled &&
            string.IsNullOrWhiteSpace(options.Dashboard.PathMatch))
        {
            return false;
        }

        return true;
    }
}
```

CAP Dashboard 可以查看消息和失败状态；官方文档说明 Dashboard 默认不是开启的，需要 `UseDashboard()`，默认路径是 `/cap`，并且 8.0.0 以后支持基于 ASP.NET Core 认证授权策略控制访问。生产环境不要匿名暴露 Dashboard。([CAP](https://cap.dotnetcore.xyz/user-guide/en/monitoring/dashboard/?utm_source=chatgpt.com "Dashboard \- CAP"))

---

# 6. 业务服务接入示例

假设你有一个 `OrderService`，里面要：

``` text
创建订单
保存订单到数据库
发布 OrderCreatedIntegrationEvent
```

## 6.1 `appsettings.json`

以 SQL Server 为例：

``` json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Ocow;User Id=sa;Password=Your_password123;TrustServerCertificate=True;"
  },
  "EventBus": {
    "DefaultGroupName": "ocow.order-service",
    "FailedRetryCount": 5,
    "FailedRetryIntervalSeconds": 60,
    "Storage": {
      "Provider": "SqlServer",
      "ConnectionStringName": "Default"
    },
    "RabbitMq": {
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "VirtualHost": "/",
      "ExchangeName": "ocow.events"
    },
    "Dashboard": {
      "Enabled": true,
      "PathMatch": "/cap",
      "AllowAnonymousExplicit": false,
      "AuthorizationPolicy": "CapDashboard"
    }
  }
}
```

生产环境注意：

``` text
不要使用 guest/guest
DefaultGroupName 每个服务要不同
Dashboard 不要匿名开放公网
```

`DefaultGroupName` 建议：

``` text
ocow.order-service
ocow.payment-service
ocow.notification-service
ocow.admin-api
```

---

## 6.2 `Program.cs`

``` csharp
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.RabbitMq;
using YourService.EventHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CapDashboard", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddOcowCapRabbitMqEventBus(builder.Configuration);

builder.Services.AddIntegrationEventHandler<
    OrderCreatedIntegrationEvent,
    OrderCreatedIntegrationEventHandler>();

builder.Services.AddTransient<OrderCreatedIntegrationEventSubscriber>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

如果当前项目还没有认证授权，可以先在开发环境临时设置：

``` json
"AllowAnonymousExplicit": true
```

但生产环境必须关掉。

---

# 7. 发布事件：强一致写法

## 7.1 错误写法

不要这样：

``` csharp
await _dbContext.SaveChangesAsync(cancellationToken);

await _eventBus.PublishAsync(
    new OrderCreatedIntegrationEvent(...),
    cancellationToken);
```

风险：

``` text
数据库保存成功
事件发布失败
```

---

## 7.2 正确写法：使用 `ICapTransactionalExecutor<TDbContext>`

``` csharp
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.RabbitMq;

namespace YourService.Orders;

public sealed class OrderApplicationService
{
    private readonly ICapTransactionalExecutor<AppDbContext> _transactionalExecutor;

    public OrderApplicationService(
        ICapTransactionalExecutor<AppDbContext> transactionalExecutor)
    {
        _transactionalExecutor = transactionalExecutor;
    }

    public Task<Guid> CreateOrderAsync(
        Guid userId,
        decimal totalAmount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        return _transactionalExecutor.ExecuteAsync<Guid>(
            async (dbContext, eventBus, ct) =>
            {
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TotalAmount = totalAmount,
                    Currency = currency,
                    CreatedAtUtc = DateTime.UtcNow
                };

                dbContext.Orders.Add(order);

                await eventBus.PublishAsync(
                    new OrderCreatedIntegrationEvent(
                        OrderId: order.Id,
                        UserId: order.UserId,
                        TotalAmount: order.TotalAmount,
                        Currency: order.Currency)
                    {
                        CorrelationId = order.Id.ToString("N")
                    },
                    ct);

                return order.Id;
            },
            cancellationToken);
    }
}
```

这段代码最终效果是：

``` text
BEGIN TRANSACTION

INSERT INTO Orders ...
INSERT INTO cap.Published ...

COMMIT
```

只要事务提交成功，业务数据和事件记录就同时存在。后续由 CAP 投递到 RabbitMQ。

---

# 8. 订阅事件

CAP 的订阅需要 `[CapSubscribe]`。第一版建议用“Adapter + Handler”模式：

``` text
CAP Subscriber Adapter
  依赖 DotNetCore.CAP
  负责接收 CAP 消息

Business Handler
  只依赖 IIntegrationEventHandler<TEvent>
  负责业务逻辑
```

---

## 8.1 业务 Handler

``` csharp
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions;

namespace YourService.EventHandlers;

public sealed class OrderCreatedIntegrationEventHandler
    : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
{
    private readonly ILogger<OrderCreatedIntegrationEventHandler> _logger;
    private readonly AppDbContext _dbContext;

    public OrderCreatedIntegrationEventHandler(
        ILogger<OrderCreatedIntegrationEventHandler> logger,
        AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task HandleAsync(
        OrderCreatedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        var alreadyProcessed = await _dbContext.ProcessedIntegrationEvents
            .AnyAsync(x => x.EventId == integrationEvent.Id, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Integration event {EventId} already processed.",
                integrationEvent.Id);

            return;
        }

        // TODO: 这里写你的消费业务逻辑
        // 比如创建通知、初始化订单扩展数据、同步搜索索引等。

        _dbContext.ProcessedIntegrationEvents.Add(new ProcessedIntegrationEvent
        {
            EventId = integrationEvent.Id,
            EventName = "ocow.orders.created",
            ProcessedAtUtc = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

消费者必须做幂等。CAP 有消费状态记录和重试，但业务逻辑仍然要能接受重复投递。

---

## 8.2 CAP Subscriber Adapter

``` csharp
using DotNetCore.CAP;
using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.Abstractions;

namespace YourService.EventHandlers;

public sealed class OrderCreatedIntegrationEventSubscriber : ICapSubscribe
{
    private readonly IIntegrationEventHandler<OrderCreatedIntegrationEvent> _handler;

    public OrderCreatedIntegrationEventSubscriber(
        IIntegrationEventHandler<OrderCreatedIntegrationEvent> handler)
    {
        _handler = handler;
    }

    [CapSubscribe("ocow.orders.created")]
    public Task HandleAsync(OrderCreatedIntegrationEvent integrationEvent)
    {
        return _handler.HandleAsync(integrationEvent);
    }
}
```

注意：CAP 官方 RabbitMQ 文档提醒，使用 RabbitMQ 时，消费者应用启动过一次后才会自动创建持久化队列；如果消费者从未启动过，发布到 exchange 的消息可能因为没有队列绑定而被丢弃。上线流程上要先部署并启动消费者，再发布依赖该消费者的新事件。([CAP](https://cap.dotnetcore.xyz/user-guide/zh/transport/rabbitmq/?utm_source=chatgpt.com "RabbitMQ \- CAP"))

---

# 9. 幂等表建议

## 9.1 Entity

``` csharp
namespace YourService.EventHandlers;

public sealed class ProcessedIntegrationEvent
{
    public Guid EventId { get; set; }

    public string EventName { get; set; } = default!;

    public DateTime ProcessedAtUtc { get; set; }
}
```

## 9.2 EF Core 配置

``` csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YourService.EventHandlers;

public sealed class ProcessedIntegrationEventConfiguration
    : IEntityTypeConfiguration<ProcessedIntegrationEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedIntegrationEvent> builder)
    {
        builder.ToTable("ProcessedIntegrationEvents");

        builder.HasKey(x => x.EventId);

        builder.Property(x => x.EventName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
            .IsRequired();
    }
}
```

这张表是业务幂等用，不是 CAP 的表。

---

# 10. CAP 表和业务表的关系

CAP 会使用自己的本地消息表，通常包含：

``` text
Published
Received
```

具体表名和结构由 CAP 存储 provider 管理。

你的业务库里会有：

``` text
Orders
ProcessedIntegrationEvents
cap.Published / cap.Received
```

创建订单时：

``` text
Orders 插入订单
cap.Published 插入事件
同一个数据库事务提交
```

消费订单事件时：

``` text
业务逻辑执行
ProcessedIntegrationEvents 插入 eventId
```

---

# 11. Docker Compose 本地开发

``` yaml
services:
  rabbitmq:
    image: rabbitmq:3.13-management
    container_name: ocow-rabbitmq
    ports:
      - "5672:5672"
      - "15672:15672"
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: ocow-sqlserver
    ports:
      - "1433:1433"
    environment:
      ACCEPT_EULA: "Y"
      SA_PASSWORD: "Your_password123"
```

启动：

``` bash
docker compose up -d
```

RabbitMQ 管理后台：

``` text
http://localhost:15672
guest / guest
```

CAP Dashboard：

``` text
http://localhost:{your-api-port}/cap
```

---

# 12. Codex 开发任务说明

你可以把下面这段直接交给 Codex。

``` text
请在当前 .NET 解决方案中实现基于 CAP + RabbitMQ 的事件总线，要求如下：

一、项目结构
1. 使用三个项目：
   - Ocow.Contracts
   - Ocow.EventBus.Abstractions
   - Ocow.EventBus.RabbitMq
2. Ocow.EventBus.RabbitMq 这个名字表示 RabbitMQ transport，但内部必须使用 CAP，不要直接封装 RabbitMQ.Client。
3. 不要引入 Ocow.EventBus.Cap 项目。

二、依赖关系
1. Ocow.Contracts 不依赖任何 EventBus 项目。
2. Ocow.EventBus.Abstractions 引用 Ocow.Contracts。
3. Ocow.EventBus.RabbitMq 引用 Ocow.Contracts 和 Ocow.EventBus.Abstractions。
4. Ocow.EventBus.RabbitMq 引入：
   - DotNetCore.CAP
   - DotNetCore.CAP.RabbitMQ
   - DotNetCore.CAP.Dashboard
   - DotNetCore.CAP.SqlServer
   - DotNetCore.CAP.PostgreSql
   - DotNetCore.CAP.MySql
   - Microsoft.EntityFrameworkCore
5. Contracts 和 Abstractions 项目不要引用 DotNetCore.CAP、RabbitMQ.Client。

三、Ocow.Contracts
新增：
1. Abstractions/IntegrationEvent.cs
   - abstract record IntegrationEvent
   - Guid Id 默认 Guid.NewGuid()
   - DateTime OccurredOnUtc 默认 DateTime.UtcNow
   - string? CorrelationId
   - string? CausationId

2. Abstractions/IntegrationEventNameAttribute.cs
   - AttributeUsage AttributeTargets.Class
   - 构造函数接收 string name
   - name 不能为空

3. Events/Orders/OrderCreatedIntegrationEvent.cs
   - 继承 IntegrationEvent
   - 使用 [IntegrationEventName("ocow.orders.created")]
   - 字段：OrderId、UserId、TotalAmount、Currency

四、Ocow.EventBus.Abstractions
新增：
1. IEventBus.cs
   - PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken)
   - PublishAsync<TEvent>(string eventName, TEvent integrationEvent, CancellationToken)
   - TEvent where TEvent : IntegrationEvent

2. IIntegrationEventHandler.cs
   - IIntegrationEventHandler<in TEvent>
   - HandleAsync(TEvent integrationEvent, CancellationToken)
   - TEvent where TEvent : IntegrationEvent

3. IIntegrationEventNameProvider.cs
   - GetName<TEvent>()
   - GetName(Type eventType)

4. DefaultIntegrationEventNameProvider.cs
   - 优先读取 IntegrationEventNameAttribute
   - 如果没有 attribute，则把 OrderCreatedIntegrationEvent 推导为 ocow.order.created
   - 校验 eventType 必须继承 IntegrationEvent

五、Ocow.EventBus.RabbitMq
新增：
1. CapRabbitMqEventBusOptions.cs
   - CapRabbitMqEventBusOptions
   - RabbitMqTransportOptions
   - CapStorageOptions
   - CapDashboardOptions

2. CapRabbitMqEventBus.cs
   - 实现 IEventBus
   - 内部使用 ICapPublisher
   - 使用 IIntegrationEventNameProvider 获取事件名
   - PublishAsync 时带 headers：
     - x-event-id
     - x-event-name
     - x-event-type
     - x-occurred-on-utc
     - x-correlation-id
     - x-causation-id

3. ICapTransactionalExecutor.cs
   - 泛型接口 ICapTransactionalExecutor<TDbContext>
   - TDbContext where TDbContext : DbContext
   - ExecuteAsync(Func<TDbContext, IEventBus, CancellationToken, Task>)
   - ExecuteAsync<TResult>(Func<TDbContext, IEventBus, CancellationToken, Task<TResult>>)

4. CapTransactionalExecutor.cs
   - 实现 ICapTransactionalExecutor<TDbContext>
   - 注入 TDbContext、ICapPublisher、IEventBus、ILogger
   - 使用 _dbContext.Database.CreateExecutionStrategy()
   - 使用 _dbContext.Database.BeginTransactionAsync(_capPublisher, autoCommit: false, cancellationToken)
   - 在同一事务里执行 operation、SaveChangesAsync、CommitAsync
   - 异常时 RollbackAsync

5. IntegrationEventHandlerRegistrationExtensions.cs
   - AddIntegrationEventHandler<TEvent, THandler>()
   - 注册 IIntegrationEventHandler<TEvent>, THandler

6. CapRabbitMqEventBusServiceCollectionExtensions.cs
   - AddOcowCapRabbitMqEventBus(IConfiguration configuration, string sectionName = "EventBus")
   - 绑定 CapRabbitMqEventBusOptions
   - ValidateOnStart
   - 注册：
     - IIntegrationEventNameProvider -> DefaultIntegrationEventNameProvider singleton
     - IEventBus -> CapRabbitMqEventBus scoped
     - ICapTransactionalExecutor<> -> CapTransactionalExecutor<> scoped
   - services.AddCap(...)
   - 配置 DefaultGroupName
   - 配置 FailedRetryCount
   - 配置 FailedRetryInterval
   - 根据 EventBus:Storage:Provider 选择 UseSqlServer / UsePostgreSql / UseMySql
   - 配置 UseRabbitMQ
   - Dashboard.Enabled 为 true 时 UseDashboard

六、业务使用示例
1. appsettings.json 增加 EventBus 配置：
   - DefaultGroupName
   - FailedRetryCount
   - FailedRetryIntervalSeconds
   - Storage.Provider
   - Storage.ConnectionStringName
   - RabbitMq.HostName
   - RabbitMq.Port
   - RabbitMq.UserName
   - RabbitMq.Password
   - RabbitMq.VirtualHost
   - RabbitMq.ExchangeName
   - Dashboard.Enabled
   - Dashboard.PathMatch
   - Dashboard.AllowAnonymousExplicit
   - Dashboard.AuthorizationPolicy

2. Program.cs 调用：
   builder.Services.AddOcowCapRabbitMqEventBus(builder.Configuration);
   builder.Services.AddIntegrationEventHandler<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
   builder.Services.AddTransient<OrderCreatedIntegrationEventSubscriber>();

3. 新增 OrderCreatedIntegrationEventHandler：
   - 实现 IIntegrationEventHandler<OrderCreatedIntegrationEvent>
   - 业务处理前检查 ProcessedIntegrationEvents 是否已处理
   - 处理成功后写入 ProcessedIntegrationEvents

4. 新增 OrderCreatedIntegrationEventSubscriber：
   - 实现 ICapSubscribe
   - 使用 [CapSubscribe("ocow.orders.created")]
   - 注入 IIntegrationEventHandler<OrderCreatedIntegrationEvent>
   - 调用 handler.HandleAsync

七、强一致发布示例
1. 业务服务通过 ICapTransactionalExecutor<AppDbContext> 创建订单并发布事件。
2. 不允许先 SaveChangesAsync 再单独 PublishAsync。
3. 正确写法是：
   await _transactionalExecutor.ExecuteAsync(async (dbContext, eventBus, ct) =>
   {
       dbContext.Orders.Add(order);
       await eventBus.PublishAsync(new OrderCreatedIntegrationEvent(...), ct);
   }, cancellationToken);

八、生产约束
1. 每个事件必须继承 IntegrationEvent。
2. 每个事件必须显式标注 IntegrationEventNameAttribute。
3. 事件名上线后不能随便改。
4. 消费者必须做幂等。
5. 生产环境不要使用 guest/guest。
6. 每个服务的 DefaultGroupName 必须不同。
7. Dashboard 生产环境不得匿名公网暴露。
8. 不要在业务层直接依赖 RabbitMQ.Client。
9. 尽量不要在业务层直接依赖 ICapPublisher，除非是基础设施层封装。
10. 确保项目可编译，启用 nullable reference types。
```

---

# 13. 最终落地建议

你这版建议定义为：

``` text
Ocow.EventBus.RabbitMq
= CAP + RabbitMQ 的生产事件总线实现
```

不是：

``` text
RabbitMQ.Client 手写事件总线
```

最终调用链是：

``` text
业务服务
  ↓
ICapTransactionalExecutor<TDbContext>
  ↓
IEventBus
  ↓
ICapPublisher
  ↓
CAP Outbox 本地消息表
  ↓
RabbitMQ
  ↓
[CapSubscribe] Subscriber
  ↓
IIntegrationEventHandler<TEvent>
```

这样可以同时满足：

``` text
1. 有清晰的事件契约：Ocow.Contracts
2. 业务侧有稳定抽象：Ocow.EventBus.Abstractions
3. 底层使用 CAP + RabbitMQ：Ocow.EventBus.RabbitMq
4. 数据库事务和事件发布强一致：CAP transaction + Outbox
5. 消费端可重试、可监控、可人工处理失败消息：CAP Dashboard
```

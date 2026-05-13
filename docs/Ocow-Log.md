
``` text
Ocow.Observability
负责：日志接入、日志规范、TraceId 关联、Serilog/OpenTelemetry 配置、公共日志扩展

各业务服务
负责：具体业务日志内容
```

---

## 推荐放在 `Ocow.Observability` 的内容

比如这些适合放进去：

``` text
Ocow.Observability
├── Extensions
│   ├── ObservabilityExtensions.cs
│   ├── LoggingExtensions.cs
│   └── SerilogExtensions.cs
│
├── Options
│   ├── ObservabilityOptions.cs
│   └── LoggingOptions.cs
│
├── Logging
│   ├── LogConstants.cs
│   ├── LogEnricher.cs
│   ├── RequestLogMiddleware.cs
│   └── SensitiveDataDestructuringPolicy.cs
│
├── Tracing
│   └── OcowActivitySource.cs
│
└── Metrics
    └── OcowMeter.cs
```

这些属于**基础设施级日志能力**，可以统一封装。

---

## 不建议放进 `Ocow.Observability` 的内容

这些不要放进去：

``` text
订单创建成功日志
用户登录失败日志
支付回调失败日志
库存扣减失败日志
优惠券核销日志
```

这些应该留在各自业务服务里，比如：

``` text
Ocow.Order.Application
Ocow.Identity.Application
Ocow.Order.Api
Ocow.Identity.Api
```

因为它们属于业务语义，不属于通用可观测能力。

---

# 推荐职责划分

## `Ocow.Observability` 负责这些

### 1. 日志框架初始化

比如统一配置 Serilog：

``` csharp
builder.Host.UseSerilog(...);
```

或者统一接入 OpenTelemetry Logs：

``` csharp
builder.Logging.AddOpenTelemetry(...);
```

---

### 2. 统一日志格式

例如所有服务日志都带上：

``` text
ServiceName
ServiceVersion
Environment
TraceId
SpanId
UserId
TenantId
RequestId
MachineName
Exception
```

这样排查问题时，你能通过一个 `TraceId` 串起来：

``` text
Gateway 日志
Order.Api 日志
Identity.Api 日志
数据库调用 Trace
消息队列 Trace
```

---

### 3. 请求日志中间件

比如统一记录：

``` text
请求路径
请求方法
响应状态码
耗时
客户端 IP
UserAgent
TraceId
UserId
TenantId
```

可以放：

``` text
Ocow.Observability.Logging.RequestLogMiddleware
```

然后每个服务只需要：

``` csharp
app.UseOcowRequestLogging();
```

---

### 4. 敏感字段脱敏

比如统一过滤：

``` text
password
token
authorization
refreshToken
idCard
bankCard
phone
```

这个也适合放在 `Ocow.Observability`。

---

### 5. 日志输出目标配置

比如统一支持：

``` text
Console
File
Seq
ElasticSearch
Loki
OpenTelemetry Collector
```

这些都可以通过配置开关控制。

---

# 推荐配置示例

``` json
{
  "Observability": {
    "ServiceName": "Ocow.Order.Api",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://otel-collector:4317",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableLogging": true
  },
  "Logging": {
    "MinimumLevel": "Information",
    "EnableConsole": true,
    "EnableFile": false,
    "EnableOpenTelemetry": true,
    "EnableRequestLogging": true
  }
}
```

也可以把 `Logging` 合进 `Observability`：

``` json
{
  "Observability": {
    "ServiceName": "Ocow.Order.Api",
    "ServiceVersion": "1.0.0",
    "OtlpEndpoint": "http://otel-collector:4317",
    "Logging": {
      "MinimumLevel": "Information",
      "EnableConsole": true,
      "EnableOpenTelemetry": true,
      "EnableRequestLogging": true
    }
  }
}
```

我更建议第二种，配置更集中。

---

# 推荐扩展方法

你可以在 `Ocow.Observability` 里提供两个入口：

``` csharp
builder.Host.UseOcowSerilog(builder.Configuration, "Ocow.Order.Api");

builder.Services.AddOcowObservability(
    builder.Configuration,
    serviceName: "Ocow.Order.Api");
```

或者统一成一个：

``` csharp
builder.AddOcowObservability("Ocow.Order.Api");
```

每个服务就很干净：

``` csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability("Ocow.Order.Api");

builder.Services.AddControllers();

var app = builder.Build();

app.UseOcowRequestLogging();

app.MapControllers();

app.Run();
```

---

# 日志和 Trace 的关系

日志不只是单独记录文本，最好和 Trace 关联起来。

例如一次请求：

``` text
TraceId: 4bf92f3577b34da6a3ce929d0e0e4736
```

那么这些地方都应该带这个 TraceId：

``` text
Gateway 请求日志
Order.Api 业务日志
EF Core SQL Trace
RabbitMQ 发布消息 Trace
消费者处理日志
```

这样你查问题时，不是只看一行日志，而是能看到完整链路：

``` text
请求从哪来
经过哪些服务
哪个服务慢
哪一步异常
对应的业务日志是什么
```

所以日志、链路追踪、指标监控都放在 `Ocow.Observability` 这个公共模块里是合理的。

---

# 最终建议

你的 `Ocow.Observability` 可以统一负责：

``` text
Tracing：链路追踪
Metrics：指标监控
Logging：日志接入
Correlation：TraceId / RequestId 关联
Request Logging：请求日志
Sensitive Data Masking：敏感信息脱敏
```

但业务日志本身仍然写在业务服务里：

``` csharp
_logger.LogInformation(
    "Order created. OrderId: {OrderId}, UserId: {UserId}",
    order.Id,
    userId);
```

一句话：**日志框架、日志规范、日志上报、TraceId 关联放 `Ocow.Observability`；具体业务日志写在各个业务模块。**

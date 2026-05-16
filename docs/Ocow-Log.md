
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




``` plain
.NET 微服务
  ↓
  OpenTelemetry SDK
    ↓
Serilog JSON Console
  ↓
OpenTelemetry Collector  
  ↓
Loki
  ↓
Grafana
```
 
2:不同环境可配置对应插件是否开启

## 你的方案总体评价

你现在的分工是：

```
Ocow.Jobs只负责定时任务触发Ocow.Order负责订单同步业务、调用 ERP、写订单表Ocow.ERP负责封装各种外部 ERP 接口
```

这个分配是合理的。核心原因是：**订单入库属于 Order 领域，不属于 Jobs 领域**。Jobs 不应该知道订单表结构、订单幂等规则、订单状态映射、客户/商品/金额字段怎么处理。

所以更推荐的调用链是：

```
定时任务页面配置   ↓Ocow.Jobs.Api 定时触发   ↓内部 JWT 调用 Ocow.Order.Api   ↓Ocow.Order.Application 执行订单同步用例   ↓调用 Ocow.ERP 适配器获取外部订单   ↓Order 领域校验、幂等、入库
```

---

## 我建议的服务边界

### 1. Ocow.Jobs：只负责“什么时候执行”

它应该负责：

```
任务配置任务启停任务调度任务执行记录失败重试手动触发
```

它不应该负责：

```
ERP 接口细节订单字段映射订单表写入订单业务校验订单状态转换
```

也就是说，Jobs 里最多保存这样的任务配置：

```
{  "jobName": "同步某ERP订单",  "jobType": "OrderErpSync",  "cron": "*/5 * * * *",  "targetService": "Order",  "targetApi": "/internal/orders/erp-sync/run",  "enabled": true}
```

Jobs 执行时只做：

```
到点了 → 带内部服务 Token 调 Order 内部接口
```

---

### 2. Ocow.Order：负责“订单同步这个业务用例”

Order 服务应该暴露一个内部接口，例如：

```
POST /internal/order/erp-sync/run
```

请求可以是：

```
{  "syncConfigId": "xxx",  "erpCode": "kingdee",  "fromTime": "2026-05-17T10:00:00Z",  "toTime": "2026-05-17T10:05:00Z"}
```

Order 服务内部做：

```
读取订单同步配置调用 ERP 适配器转换外部订单为内部订单根据 ExternalOrderId 做幂等插入或更新订单表记录同步结果
```

这里有一个关键点：**LastSyncTime 更建议由 Order 管，不建议由 Jobs 管。**

因为“同步到哪里了”是订单同步业务状态，不是调度器状态。Jobs 只知道“我触发过一次”，Order 才知道“我成功同步到了哪个时间点”。

---

### 3. Ocow.ERP：只负责“外部 ERP 接口适配”

这个包可以设计成这样：

```
public interface IErpOrderClient{    Task<IReadOnlyList<ExternalErpOrderDto>> GetOrdersAsync(        ErpConnectionOptions options,        DateTimeOffset fromTime,        DateTimeOffset toTime,        CancellationToken cancellationToken);}
```

然后不同 ERP 一个实现：

```
KingdeeErpOrderClientYonyouErpOrderClientSapErpOrderClientCustomErpOrderClient
```

再用 Factory 分发：

```
public interface IErpClientFactory{    IErpOrderClient Create(string erpCode);}
```

Order 只依赖抽象：

```
var client = _erpClientFactory.Create(config.ErpCode);var orders = await client.GetOrdersAsync(options, fromTime, toTime, cancellationToken);
```

这样后面新增 ERP，不需要改 Jobs，也尽量少改 Order 主流程。

---

## 关于 Ocow.Jobs 要不要改成 DDD

我的建议是：**可以按分层架构改，但没必要上完整 DDD。**

因为 Jobs 这个服务本质上偏“调度平台/基础设施服务”，不是特别典型的业务域。它可以有这些层：

```
Ocow.Jobs.DomainOcow.Jobs.ApplicationOcow.Jobs.InfrastructureOcow.Jobs.Api
```

但不要为了 DDD 而 DDD。你真正需要的是：

```
ControllersApplication ServicesDomain ModelsInfrastructureBackground Worker
```

核心对象可以是：

```
JobDefinition        -- 任务定义JobTrigger           -- 触发配置JobExecutionLog      -- 执行日志JobExecutionResult   -- 执行结果
```

如果你们项目已经有统一 DDD 模板，那可以改；如果没有，建议先做“清晰分层”，不要先大改框架。

---

## 内部服务 JWT 通讯认证，应该看这个模块

从你截图的结构看，最可能负责内部服务间 JWT 通讯的是：

```
src/BuildingBlocks/Ocow.InternalAuth
```

其次可能相关的是：

```
src/BuildingBlocks/Ocow.Authsrc/BuildingBlocks/Ocow.AspNetCore
```

我的判断：

```
Ocow.InternalAuth大概率负责服务间认证，比如内部 JWT、服务身份、内部接口鉴权。Ocow.Auth更可能负责用户登录、用户 JWT、权限、Claims、前后台登录认证。Ocow.AspNetCore可能是 ASP.NET Core 的扩展注册，比如 AddAuthentication、UseAuthorization、统一中间件等。
```

所以你要分析内部服务 JWT，优先看：

```
Ocow.InternalAuth
```

重点找这些类或关键词：

```
InternalAuthInternalJwtServiceTokenServiceAuthenticationInternalAuthenticationHandlerInternalAuthorizeServiceJwtOptionsJwtBearerClaimsPrincipalAddInternalAuthUseInternalAuth
```

如果有类似扩展方法：

```
services.AddInternalAuth(...)
```

或者：

```
app.UseInternalAuth()
```

那基本就是它。

---

## 内部 JWT 调用建议

Jobs 调 Order 时，不建议复用普通用户 Token。应该使用 **服务身份 Token**。

例如 Token Claims：

```
{  "sub": "ocow.jobs",  "client_id": "ocow.jobs",  "scope": "internal.order.sync",  "service": "jobs",  "aud": "ocow.order",  "iss": "ocow.internal"}
```

Order 的内部接口加策略：

```
[Authorize(Policy = "InternalService")][HttpPost("/internal/order/erp-sync/run")]public async Task<IActionResult> RunErpSyncAsync(...){    ...}
```

并且最好再限制 scope：

```
internal.order.sync
```

不要只要是内部服务 Token 就能调用所有内部接口。

---

## 一个更稳的最终架构

我建议你落成这个版本：

```
Ocow.Jobs.Api  - 管理任务配置  - 每 5 分钟触发任务  - 使用 InternalAuth 生成服务 JWT  - 调用 Ocow.Order 内部接口  - 记录触发结果，不处理订单业务Ocow.Order.Api  - 提供内部同步接口  - 执行订单同步 Application Service  - 调 ERP 包  - 做订单幂等、状态映射、金额校验、入库  - 维护 LastSyncTime 和同步日志Ocow.ERP  - 定义 ERP Client 抽象  - 每种 ERP 一个实现  - 只返回标准化 ExternalOrderDto  - 不依赖订单业务表
```

---

## 我会特别注意这几个坑

### 1. 不要让 Jobs 保存订单同步游标

不要这样：

```
Jobs 保存 LastSyncTime
```

更建议：

```
Order 保存 LastSyncTime
```

因为如果 Order 拉取成功但写入失败，或者部分成功，游标推进会导致漏单。

---

### 2. 订单入库必须幂等

订单表或同步映射表一定要有：

```
SourceSystemExternalOrderId
```

并建立唯一约束：

```
UNIQUE(SourceSystem, ExternalOrderId)
```

否则每 5 分钟拉一次，一定会重复插入。

---

### 3. 拉取窗口要有回拨

不要只拉：

```
上次同步时间 ~ 当前时间
```

建议拉：

```
上次同步时间 - 5分钟 ~ 当前时间
```

然后靠幂等去重。这样可以避免 ERP 延迟、网络延迟、时间偏差导致漏单。

---

### 4. Order 调 ERP，不要让 ERP 包写数据库

`Ocow.ERP` 只能负责：

```
HTTP 请求签名鉴权分页反序列化ERP 字段标准化
```

不要让它做：

```
写订单表查业务库判断订单状态处理业务规则
```

---

### 5. Jobs 调 Order 最好支持手动触发

除了定时任务，建议页面加一个按钮：

```
立即执行一次
```

这会极大方便测试和排查。


是的，**Ocow.Jobs 应该有自己的任务配置表**。否则页面配置、启停、修改频率、手动执行、查看执行历史都没有持久化来源。

你列的这些字段是最小可用版本，但我建议拆成两类表：

``` text
1. JobDefinition / JobConfig：记录“这个任务是什么、怎么调度、调哪个服务”
2. JobExecutionLog：记录“每次执行结果如何”
```

---

## 1. 任务配置表：JobDefinition

可以建一张类似：

``` sql
CREATE TABLE job_definitions (
    id                  uuid PRIMARY KEY,
    job_name            varchar(100) NOT NULL,
    job_type            varchar(100) NOT NULL,
    cron_expression     varchar(100) NOT NULL,
    target_service      varchar(100) NOT NULL,
    target_api          varchar(300) NOT NULL,
    http_method         varchar(10) NOT NULL DEFAULT 'POST',
    request_body        text NULL,
    enabled             boolean NOT NULL DEFAULT true,

    last_trigger_time   timestamptz NULL,
    next_trigger_time   timestamptz NULL,

    timeout_seconds     int NOT NULL DEFAULT 60,
    retry_count         int NOT NULL DEFAULT 0,

    description         varchar(500) NULL,

    created_at          timestamptz NOT NULL,
    updated_at          timestamptz NOT NULL
);
```

对应你的 JSON，大概就是：

``` json
{
  "jobName": "同步某ERP订单",
  "jobType": "OrderErpSync",
  "cronExpression": "*/5 * * * *",
  "targetService": "Order",
  "targetApi": "/internal/orders/erp-sync/run",
  "httpMethod": "POST",
  "enabled": true
}
```

我建议补上 `httpMethod`、`timeoutSeconds`、`lastTriggerTime`、`nextTriggerTime`。  
这几个字段实际运维时非常有用。

---

## 2. 执行日志表：JobExecutionLog

定时任务一定要有执行日志，不然后面排查不了问题。

``` sql
CREATE TABLE job_execution_logs (
    id                  uuid PRIMARY KEY,
    job_definition_id   uuid NOT NULL,
    job_name            varchar(100) NOT NULL,
    job_type            varchar(100) NOT NULL,

    status              varchar(30) NOT NULL,
    start_time          timestamptz NOT NULL,
    end_time            timestamptz NULL,
    duration_ms         bigint NULL,

    target_service      varchar(100) NOT NULL,
    target_api          varchar(300) NOT NULL,
    http_status_code    int NULL,

    request_body        text NULL,
    response_body       text NULL,
    error_message       text NULL,

    triggered_by        varchar(50) NOT NULL DEFAULT 'Scheduler',

    created_at          timestamptz NOT NULL
);
```

`status` 可以定义为：

``` text
Running
Success
Failed
Timeout
Canceled
Skipped
```

---

## 3. 你的这几个字段是否够？

你的字段：

``` json
{
  "jobName": "同步某ERP订单",
  "jobType": "OrderErpSync",
  "cron": "*/5 * * * *",
  "targetService": "Order",
  "targetApi": "/internal/orders/erp-sync/run",
  "enabled": true
}
```

作为 MVP 是够的。

但我建议至少增加这些：

``` json
{
  "httpMethod": "POST",
  "timeoutSeconds": 60,
  "retryCount": 0,
  "lastTriggerTime": null,
  "nextTriggerTime": null,
  "description": "每5分钟同步ERP订单"
}
```

完整一点：

``` json
{
  "jobName": "同步某ERP订单",
  "jobType": "OrderErpSync",
  "cronExpression": "*/5 * * * *",
  "targetService": "Order",
  "targetApi": "/internal/orders/erp-sync/run",
  "httpMethod": "POST",
  "requestBody": {
    "erpCode": "kingdee"
  },
  "enabled": true,
  "timeoutSeconds": 60,
  "retryCount": 0,
  "description": "每5分钟从金蝶ERP同步订单"
}
```

---

## 4. requestBody 要不要存在 Jobs 表里？

这个要看你的设计。

### 方案 A：Jobs 表只存调度信息

``` json
{
  "jobType": "OrderErpSync",
  "targetService": "Order",
  "targetApi": "/internal/orders/erp-sync/run"
}
```

具体同步哪个 ERP、同步哪个店铺、同步哪些订单，由 Order 服务自己的配置表管理。

这是我更推荐的方式。

也就是说，Jobs 只传：

``` json
{
  "jobKey": "order-erp-sync"
}
```

Order 自己根据 `jobKey` 找同步配置。

优点是职责更干净：

``` text
Jobs 不关心 ERP 配置
Order 自己管理订单同步配置
```

---

### 方案 B：Jobs 表存 requestBody

``` json
{
  "erpCode": "kingdee",
  "tenantId": "xxx",
  "syncConfigId": "xxx"
}
```

这样 Jobs 可以配置多个类似任务：

``` text
同步金蝶订单
同步用友订单
同步某客户ERP订单
```

这个也可以，但要注意：**Jobs 只存参数，不存 ERP 密钥**。

不要把这些放 Jobs 表：

``` text
appKey
appSecret
accessToken
erpPassword
```

这些应该在 Order 或 ERP 连接配置里加密存储。

---

## 5. 我更推荐的最终设计

### Jobs 表记录

``` json
{
  "jobName": "同步ERP订单",
  "jobCode": "order-erp-sync",
  "jobType": "HttpCallback",
  "cronExpression": "*/5 * * * *",
  "targetService": "Order",
  "targetApi": "/internal/orders/erp-sync/run",
  "httpMethod": "POST",
  "enabled": true
}
```

### Order 服务自己记录 ERP 同步配置

``` json
{
  "syncConfigName": "金蝶订单同步",
  "erpCode": "kingdee",
  "enabled": true,
  "lastSyncTime": "2026-05-17T10:00:00Z"
}
```

### Jobs 到点后调用 Order

``` http
POST /internal/orders/erp-sync/run
```

body：

``` json
{
  "jobCode": "order-erp-sync"
}
```

然后 Order 服务自己决定：

``` text
查启用的 ERP 同步配置
逐个调用 ERP
同步订单
更新 LastSyncTime
记录订单同步日志
```

---

## 结论

**要加表。**

最少加两张：

``` text
job_definitions       -- 任务定义/配置
job_execution_logs    -- 任务执行日志
```

你列的字段可以作为 `job_definitions` 的核心字段。  
但我建议 Jobs 只保存“调度和调用目标”，不要保存 ERP 密钥、订单同步游标、订单业务状态。

最稳的边界是：

``` text
Ocow.Jobs 记录：什么时候调、调哪个服务、调哪个接口
Ocow.Order 记录：同步哪个 ERP、同步到哪里、订单如何入库
```

这样后期维护会清楚很多。

## 1. 项目结构分层

``` text
BuildingBlocks 基础层
Gateways 网关层
Services 业务服务层
tests 测试层
```


---

## 2. 项目结构总览

``` text
src
├── BuildingBlocks
│   ├── Ocow.Shared
│   ├── Ocow.Contracts
│   ├── Ocow.AspNetCore
│   ├── Ocow.Auth
│   ├── Ocow.InternalAuth
│   ├── Ocow.EntityFrameworkCore
│   ├── Ocow.Redis
│   ├── Ocow.MessageBus
│   ├── Ocow.Observability
│   ├── Ocow.HealthChecks
│   └── Ocow.ServiceDiscovery    可选
│
├── Gateways
│   └── Ocow.Gateway
│
├── Services
│   ├── Identity
│   │   ├── Ocow.Identity.Api
│   │   ├── Ocow.Identity.Application
│   │   ├── Ocow.Identity.Domain
│   │   ├── Ocow.Identity.Infrastructure
│   │   └── Ocow.Identity.Migrations
│   │
│   ├── Member
│   │   ├── Ocow.Member.Api
│   │   ├── Ocow.Member.Application
│   │   ├── Ocow.Member.Domain
│   │   ├── Ocow.Member.Infrastructure
│   │   └── Ocow.Member.Migrations
│   │
│   ├── Product
│   │   ├── Ocow.Product.Api
│   │   ├── Ocow.Product.Application
│   │   ├── Ocow.Product.Domain
│   │   ├── Ocow.Product.Infrastructure
│   │   └── Ocow.Product.Migrations
│   │
│   ├── Inventory
│   │   ├── Ocow.Inventory.Api
│   │   ├── Ocow.Inventory.Application
│   │   ├── Ocow.Inventory.Domain
│   │   ├── Ocow.Inventory.Infrastructure
│   │   └── Ocow.Inventory.Migrations
│   │
│   ├── Order
│   │   ├── Ocow.Order.Api
│   │   ├── Ocow.Order.Application
│   │   ├── Ocow.Order.Domain
│   │   ├── Ocow.Order.Infrastructure
│   │   └── Ocow.Order.Migrations
│   │
│   ├── Payment
│   │   ├── Ocow.Payment.Api
│   │   ├── Ocow.Payment.Application
│   │   ├── Ocow.Payment.Domain
│   │   ├── Ocow.Payment.Infrastructure
│   │   └── Ocow.Payment.Migrations
│   │
│   ├── WeChat
│   │   ├── Ocow.WeChat.Api
│   │   ├── Ocow.WeChat.Application
│   │   ├── Ocow.WeChat.Domain
│   │   ├── Ocow.WeChat.Infrastructure
│   │   └── Ocow.WeChat.Migrations
│   │
│   └── Scheduler
│       ├── Ocow.Scheduler.Api
│       ├── Ocow.Scheduler.Application
│       ├── Ocow.Scheduler.Domain
│       ├── Ocow.Scheduler.Infrastructure
│       └── Ocow.Scheduler.Migrations
│
└── tests
    ├── Ocow.Tests.Unit
    └── Ocow.Tests.Integration
```

---

# 3. BuildingBlocks 基础层

`BuildingBlocks` 存放整个系统可复用的基础能力，原则是：**只放跨服务通用能力，不放具体业务逻辑**。

## 3.1 基础层项目清单

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Shared` | 公共基础能力，提供统一返回、分页模型、基础异常、通用工具类、时间、雪花 ID、Guard、常量等 |
| `Ocow.Contracts` | 跨服务契约，放集成事件、公共消息模型、跨服务 DTO、枚举契约等 |
| `Ocow.AspNetCore` | Web API 通用封装，提供全局异常处理中间件、统一响应处理、Swagger、CORS、模型验证、HttpContext 扩展等 |
| `Ocow.Auth` | 用户认证与授权公共封装，提供 JWT Bearer 配置、Claims 解析、当前用户上下文、权限策略等 |
| `Ocow.InternalAuth` | 内部服务认证，提供 Service JWT、HMAC 签名、内部接口鉴权、服务间调用身份校验 |
| `Ocow.EntityFrameworkCore` | EF Core 通用封装，提供基础实体接口、审计字段、软删除、DbContext 基类、UnitOfWork、多数据库 Provider 扩展等 |
| `Ocow.Redis` | Redis 公共封装，提供缓存、分布式锁、限流、幂等 Key 存储等能力 |
| `Ocow.MessageBus` | RabbitMQ 公共封装，提供发布订阅、集成事件、重试、死信队列、序列化、Outbox / Inbox 支持 |
| `Ocow.Observability` | 可观测性公共封装，提供日志、链路追踪、指标监控、OpenTelemetry、TraceId / RequestId 关联等 |
| `Ocow.HealthChecks` | 健康检查公共封装，提供 `/health`、`/live`、`/ready`，检查数据库、Redis、RabbitMQ 等依赖状态 |
| `Ocow.ServiceDiscovery` | 服务注册发现封装，可选项目，适配 Consul、Nacos 或 Kubernetes DNS；Docker Compose / K8S 场景通常不需要单独实现 |

---

## 3.2 `Ocow.Shared`

`Ocow.Shared` 是最基础的公共项目，适合放不依赖具体框架的通用能力。

建议包含：

``` text
Ocow.Shared
├── Constants
├── Exceptions
├── Models
├── Results
├── Pagination
├── Helpers
├── Extensions
└── Utilities
```

适合放：

``` text
统一返回模型
分页模型
基础异常
Guard 校验
日期时间工具
字符串扩展
集合扩展
雪花 ID
通用常量
```

不建议放：

``` text
JWT 认证
EF Core 逻辑
Redis 逻辑
RabbitMQ 逻辑
具体业务 DTO
```

---

## 3.3 `Ocow.Contracts`

`Ocow.Contracts` 用于定义跨服务通信的契约。

建议包含：

``` text
Ocow.Contracts
├── Events
│   ├── Order
│   ├── Payment
│   ├── Product
│   └── Member
├── Messages
├── Requests
├── Responses
└── Enums
```

适合放：

``` text
集成事件
跨服务消息模型
跨服务请求响应 DTO
公共枚举契约
```

例如：

``` text
OrderCreatedEvent
OrderPaidEvent
PaymentSucceededEvent
InventoryLockedEvent
MemberRegisteredEvent
```

注意：`Contracts` 不应该依赖具体业务服务的 `Domain` 项目。

---

## 3.4 `Ocow.AspNetCore`

`Ocow.AspNetCore` 负责 ASP.NET Core Web API 层的通用能力。

建议包含：

``` text
Ocow.AspNetCore
├── Extensions
├── Middleware
├── Filters
├── Models
├── Options
└── Swagger
```

适合放：

``` text
全局异常处理中间件
统一错误响应
模型验证错误处理
Swagger 配置
CORS 配置
Controller 扩展
HttpContext 扩展
请求上下文
API 行为配置
```

每个 API 项目可以这样使用：

``` csharp
builder.Services.AddOcowWebApi(builder.Configuration);

app.UseOcowExceptionHandling();
app.UseOcowRequestContext();
```

---

## 3.5 `Ocow.Auth`

`Ocow.Auth` 负责用户身份认证与用户权限相关能力。

建议包含：

``` text
Ocow.Auth
├── Attributes
├── Constants
├── Extensions
├── Interfaces
├── Models
├── Options
├── Requirements
└── Services
```

适合放：

``` text
JWT Bearer 配置
JwtOptions
Claim 常量
ICurrentUser
CurrentUser
权限策略
Permission Requirement
Authorization Handler
用户身份解析
```

适用场景：

``` text
小程序用户访问接口
PC 后台管理员访问接口
Order.Api 校验用户 JWT
Product.Api 校验后台权限
Gateway 统一校验 Token
```

注意：`Ocow.Auth` 只负责“识别和校验 Token”，不负责登录业务。

登录、注册、刷新 Token 这些业务仍然放在：

``` text
Ocow.Identity.Application
Ocow.Identity.Api
```

---

## 3.6 `Ocow.InternalAuth`

`Ocow.InternalAuth` 负责服务间认证，不建议放普通用户 JWT 认证。

建议包含：

``` text
Ocow.InternalAuth
├── Attributes
├── Constants
├── Extensions
├── Filters
├── Interfaces
├── Models
├── Options
├── Requirements
└── Services
```

适合放：

``` text
Service JWT
HMAC 签名
内部接口鉴权
服务调用方身份
内部任务调用认证
内部 Header 校验
```

适用场景：

``` text
Order.Api 调用 Inventory.Api
Payment.Api 回调后通知 Order.Api
Scheduler.Api 调用内部同步接口
消息消费者调用内部处理接口
```

示例：

``` csharp
[InternalOnly]
[HttpPost("sync-order-status")]
public async Task<IActionResult> SyncOrderStatus()
{
    ...
}
```

---

## 3.7 `Ocow.EntityFrameworkCore`

`Ocow.EntityFrameworkCore` 负责 EF Core 通用能力封装。

建议包含：

``` text
Ocow.EntityFrameworkCore
├── Abstractions
├── Entities
├── Interceptors
├── Repositories
├── UnitOfWork
├── Extensions
├── Options
└── Providers
```

适合放：

``` text
实体基础接口
审计字段
软删除
多租户字段
DbContext 基类
Repository 基类
UnitOfWork
SaveChanges 拦截器
CreatedAt / UpdatedAt 自动填充
数据库 Provider 扩展
```

例如：

``` text
IAuditableEntity
ISoftDelete
IHasTenantId
EfCoreDbContextBase
AuditableSaveChangesInterceptor
```

---

## 3.8 `Ocow.Redis`

`Ocow.Redis` 负责 Redis 相关能力。

建议包含：

``` text
Ocow.Redis
├── Abstractions
├── Caching
├── DistributedLocks
├── RateLimiting
├── Idempotency
├── Options
├── Serialization
└── Extensions
```

适合放：

``` text
Redis 连接管理
缓存封装
分布式锁
限流计数器
幂等 Key 存储
Redis 序列化
Redis Key 规范
```

如果后期缓存能力变复杂，可以再拆：

``` text
Ocow.Caching
```

当前阶段直接放在 `Ocow.Redis` 里也可以。

---

## 3.9 `Ocow.MessageBus`

`Ocow.MessageBus` 负责消息队列和事件通信。

建议包含：

``` text
Ocow.MessageBus
├── Abstractions
├── Events
├── RabbitMQ
├── Outbox
├── Inbox
├── Serialization
├── Retry
├── Options
└── Extensions
```

适合放：

``` text
IMessageBus
IEventBus
IntegrationEvent
事件发布
事件订阅
RabbitMQ 封装
消息序列化
重试策略
死信队列
Outbox 本地消息表
Inbox 消费幂等
```

典型事件：

``` text
OrderCreatedEvent
OrderPaidEvent
PaymentSucceededEvent
InventoryDeductedEvent
```

---

## 3.10 `Ocow.Observability`

`Ocow.Observability` 负责可观测性能力。

建议包含：

``` text
Ocow.Observability
├── Logging
├── Tracing
├── Metrics
├── Correlation
├── Exporters
├── Options
└── Extensions
```

适合放：

``` text
Serilog 配置
OpenTelemetry 配置
Trace Span 上报
Metrics 指标上报
RequestId / TraceId 关联
请求日志中间件
日志脱敏
OTLP Exporter 配置
ActivitySource
Meter
```

每个服务可以统一接入：

``` csharp
builder.Services.AddOcowObservability(
    builder.Configuration,
    serviceName: "Ocow.Order.Api");
```

---

## 3.11 `Ocow.HealthChecks`

`Ocow.HealthChecks` 负责健康检查。

建议包含：

``` text
Ocow.HealthChecks
├── Extensions
├── Options
├── Publishers
└── ResponseWriters
```

适合放：

``` text
/health
/live
/ready
数据库健康检查
Redis 健康检查
RabbitMQ 健康检查
自定义健康检查响应
```

区别：

``` text
/live  检查进程是否存活
/ready 检查服务是否准备好接收流量
/health 返回完整健康状态
```

Docker / K8S / Nginx / 负载均衡都可以使用这些接口判断服务状态。

---

## 3.12 `Ocow.ServiceDiscovery` 可选

`Ocow.ServiceDiscovery` 不是必须项目。

如果使用 Docker Compose：

``` text
服务发现由 Docker 内部 DNS 完成
```

如果使用 K8S：

``` text
服务发现由 Kubernetes Service + CoreDNS 完成
```

只有在多台虚拟机、非 K8S 部署、需要 Consul / Nacos 时，才建议启用该项目。

建议包含：

``` text
Ocow.ServiceDiscovery
├── Abstractions
├── Consul
├── Nacos
├── Kubernetes
├── Models
├── Options
└── Extensions
```

适合放：

``` text
服务注册
服务注销
服务查询
服务实例选择
健康检查上报
Consul 适配
Nacos 适配
Kubernetes DNS 适配
```

---

# 4. Gateways 网关层

`Gateways` 存放系统统一入口服务。

## 4.1 网关层项目清单

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Gateway` | Ocelot 网关，统一接入小程序和 PC 后台请求，负责路由转发、基础鉴权、限流、聚合、跨域、请求头透传等 |

---

## 4.2 `Ocow.Gateway`

`Ocow.Gateway` 是系统的统一 API 入口。

主要职责：

``` text
统一接入小程序请求
统一接入 PC 后台请求
路由转发到各业务服务
统一 JWT 校验
统一 CORS
统一限流
统一请求日志
统一 TraceId 透传
隐藏内部服务地址
```

典型路由：

``` text
/api/identity/**  → Ocow.Identity.Api
/api/member/**    → Ocow.Member.Api
/api/product/**   → Ocow.Product.Api
/api/order/**     → Ocow.Order.Api
/api/payment/**   → Ocow.Payment.Api
/api/wechat/**    → Ocow.WeChat.Api
```

注意：

``` text
Gateway 适合做通用鉴权
业务级鉴权仍然应该放在具体业务服务中
```

例如：

``` text
用户是否登录                     → Gateway 可以判断
用户能否查看订单 #1001           → Order.Api 判断
用户能否退款订单 #1001           → Payment.Api / Order.Api 判断
商家能否修改某个 SKU             → Product.Api 判断
```

---

# 5. Services 业务服务层

`Services` 存放具体业务微服务。每个业务服务建议按 DDD / Clean Architecture 拆成：

``` text
Ocow.Xxx.Api
Ocow.Xxx.Application
Ocow.Xxx.Domain
Ocow.Xxx.Infrastructure
Ocow.Xxx.Migrations
```

## 5.1 标准分层说明

| 项目后缀 | 说明 |
| --- | --- |
| 项目后缀 | 说明 |
| `.Api` | HTTP 入口层，提供 Controller、Minimal API、认证授权、请求模型、响应模型 |
| `.Application` | 应用层，负责编排业务用例，例如下单、取消、支付、发货、同步等 |
| `.Domain` | 领域层，包含实体、值对象、领域服务、领域事件、业务规则 |
| `.Infrastructure` | 基础设施层，包含 EF Core、仓储实现、Redis、消息队列、第三方接口适配 |
| `.Migrations` | EF Core 迁移与种子数据初始化项目，可使用 Console 项目承载 |

---

## 5.2 Identity 身份认证服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Identity.Api` | 身份认证服务 HTTP 入口，提供小程序登录、后台管理员登录、刷新 Token、获取当前用户信息等接口 |
| `Ocow.Identity.Application` | 身份认证应用层，负责编排登录、注册、刷新 Token、角色授权、权限分配等用例 |
| `Ocow.Identity.Domain` | 身份认证领域层，包含用户、管理员、角色、权限、登录账号、Token 规则等领域模型 |
| `Ocow.Identity.Infrastructure` | 身份认证基础设施层，包含 EF Core、仓储、密码哈希、Token 生成、外部登录适配等 |
| `Ocow.Identity.Migrations` | 身份认证数据库迁移与种子数据初始化项目 |

主要职责：

``` text
小程序登录
PC 后台管理员登录
Token 签发
RefreshToken 管理
角色管理
权限管理
用户身份识别
```

---

## 5.3 Member 会员服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Member.Api` | 会员服务 HTTP 入口，提供会员资料、收货地址、会员关系等接口 |
| `Ocow.Member.Application` | 会员服务应用层，负责编排会员资料维护、地址管理、会员关系维护等用例 |
| `Ocow.Member.Domain` | 会员服务领域层，包含会员实体、地址实体、会员等级、会员关系等规则 |
| `Ocow.Member.Infrastructure` | 会员服务基础设施层，包含 EF Core、仓储、缓存、外部服务适配等 |
| `Ocow.Member.Migrations` | 会员服务数据库迁移与种子数据初始化项目 |

主要职责：

``` text
会员资料
收货地址
会员等级
会员关系
用户画像基础信息
```

说明：

``` text
Identity 负责登录认证
Member 负责会员业务资料
```

---

## 5.4 Product 商品服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Product.Api` | 商品服务 HTTP 入口，提供商品、SKU、类目、品牌、上下架等接口 |
| `Ocow.Product.Application` | 商品服务应用层，负责编排商品创建、编辑、上下架、SKU 管理等用例 |
| `Ocow.Product.Domain` | 商品服务领域层，包含商品实体、SKU、类目、品牌、商品状态、价格规则等 |
| `Ocow.Product.Infrastructure` | 商品服务基础设施层，包含 EF Core、仓储、缓存、搜索引擎适配等 |
| `Ocow.Product.Migrations` | 商品服务数据库迁移与种子数据初始化项目 |

主要职责：

``` text
商品管理
SKU 管理
类目管理
品牌管理
商品上下架
商品价格
商品详情
```

---

## 5.5 Inventory 库存服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Inventory.Api` | 库存服务 HTTP 入口，提供库存查询、锁库存、扣库存、释放库存等接口 |
| `Ocow.Inventory.Application` | 库存服务应用层，负责编排库存锁定、库存扣减、库存释放、库存同步等用例 |
| `Ocow.Inventory.Domain` | 库存服务领域层，包含库存实体、库存流水、锁定库存、库存规则等 |
| `Ocow.Inventory.Infrastructure` | 库存服务基础设施层，包含 EF Core、仓储、Redis、消息队列等 |
| `Ocow.Inventory.Migrations` | 库存服务数据库迁移与种子数据初始化项目 |

主要职责：

``` text
库存查询
锁库存
扣库存
释放库存
库存流水
库存同步
```

典型流程：

``` text
下单时锁库存
支付成功后扣库存
订单取消后释放库存
```

---

## 5.6 Order 订单服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Order.Api` | 订单服务 HTTP 入口，包含小程序订单、后台订单、内部订单接口 |
| `Ocow.Order.Application` | 订单应用层，负责编排下单、取消、发货、同步、售后入口等用例 |
| `Ocow.Order.Domain` | 订单领域层，包含订单实体、订单明细、订单状态、订单金额、领域规则 |
| `Ocow.Order.Infrastructure` | 订单基础设施层，包含 EF Core、仓储、Redis、外部服务适配、消息发布等 |
| `Ocow.Order.Migrations` | 订单服务 EF Core 迁移与种子数据初始化项目 |

主要职责：

``` text
创建订单
取消订单
查询订单
订单发货
订单状态流转
订单金额计算
订单超时关闭
订单事件发布
```

典型事件：

``` text
OrderCreatedEvent
OrderCancelledEvent
OrderPaidEvent
OrderShippedEvent
OrderCompletedEvent
```

---

## 5.7 Payment 支付服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Payment.Api` | 支付服务 HTTP 入口，提供支付单创建、退款申请、支付回调、退款回调等接口 |
| `Ocow.Payment.Application` | 支付服务应用层，负责编排支付、退款、回调处理、资金状态同步等用例 |
| `Ocow.Payment.Domain` | 支付服务领域层，包含支付单、退款单、支付状态、退款状态、资金规则等 |
| `Ocow.Payment.Infrastructure` | 支付服务基础设施层，包含 EF Core、仓储、微信支付、支付宝支付、消息发布等适配 |
| `Ocow.Payment.Migrations` | 支付服务数据库迁移与种子数据初始化项目 |

主要职责：

``` text
支付单创建
微信支付
支付宝支付
支付回调
退款申请
退款回调
支付状态同步
资金流水
```

典型事件：

``` text
PaymentSucceededEvent
PaymentFailedEvent
RefundSucceededEvent
RefundFailedEvent
```

---

## 5.8 WeChat 微信集成服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.WeChat.Api` | 微信集成服务 HTTP 入口，提供小程序、公众号、订阅消息、模板消息等接口 |
| `Ocow.WeChat.Application` | 微信集成应用层，负责编排微信登录凭证解析、消息发送、微信配置管理等用例 |
| `Ocow.WeChat.Domain` | 微信集成领域层，包含微信账号、模板消息、订阅消息、发送记录等模型 |
| `Ocow.WeChat.Infrastructure` | 微信集成基础设施层，包含微信 API Client、AccessToken 缓存、消息发送适配等 |
| `Ocow.WeChat.Migrations` | 微信集成服务数据库迁移与种子数据初始化项目 |

主要职责：

``` text
小程序接口
公众号接口
订阅消息
模板消息
微信 AccessToken 管理
微信 API 适配
```

说明：

``` text
支付相关微信接口建议放在 Payment 服务
普通微信开放能力建议放在 WeChat 服务
```

---

## 5.9 Scheduler 定时任务服务

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Scheduler.Api` | 定时任务服务 HTTP 入口，提供任务管理、任务触发、任务日志查询等接口 |
| `Ocow.Scheduler.Application` | 定时任务应用层，负责编排定时任务注册、执行、重试、补偿等用例 |
| `Ocow.Scheduler.Domain` | 定时任务领域层，包含任务定义、任务状态、任务日志、执行规则等 |
| `Ocow.Scheduler.Infrastructure` | 定时任务基础设施层，包含 Hangfire、Quartz、数据库、消息队列等适配 |
| `Ocow.Scheduler.Migrations` | 定时任务服务数据库迁移与种子数据初始化项目 |

主要职责：

``` text
订单超时关闭
支付状态同步
退款状态同步
库存补偿
消息补偿
定时清理
任务执行日志
```

---

# 6. tests 测试层

## 6.1 测试项目清单

| 项目名 | 说明 |
| --- | --- |
| 项目名 | 说明 |
| `Ocow.Tests.Unit` | 单元测试项目，测试 Domain、Application 中的核心业务规则和应用用例 |
| `Ocow.Tests.Integration` | 集成测试项目，测试 API、数据库、Redis、RabbitMQ、跨服务流程等 |

---

## 6.2 单元测试

`Ocow.Tests.Unit` 适合测试：

``` text
领域实体规则
订单状态流转
金额计算
库存锁定规则
权限判断规则
应用服务编排逻辑
```

例如：

``` text
订单已发货不能取消
库存不足不能下单
支付成功后订单状态变更为已支付
退款金额不能大于支付金额
```

---

## 6.3 集成测试

`Ocow.Tests.Integration` 适合测试：

``` text
API 接口
数据库读写
Redis 缓存
RabbitMQ 消息
EF Core 迁移
服务间调用
完整业务流程
```

例如：

``` text
创建订单 → 锁库存 → 创建支付单
支付成功回调 → 订单变为已支付 → 扣减库存
订单取消 → 释放库存
```

---

# 7. 依赖关系建议

## 7.1 业务服务内部依赖

每个业务服务建议保持：

``` text
Api → Application → Domain
Infrastructure → Application / Domain
Migrations → Infrastructure
```

更具体：

``` text
Ocow.Order.Api
  引用 Ocow.Order.Application
  引用 Ocow.Order.Infrastructure

Ocow.Order.Application
  引用 Ocow.Order.Domain
  引用 Ocow.Contracts
  引用 Ocow.Shared

Ocow.Order.Infrastructure
  引用 Ocow.Order.Domain
  引用 Ocow.EntityFrameworkCore
  引用 Ocow.Redis
  引用 Ocow.MessageBus

Ocow.Order.Migrations
  引用 Ocow.Order.Infrastructure
```

---

## 7.2 BuildingBlocks 依赖建议

建议依赖方向：

``` text
Ocow.Shared
  不依赖其他 BuildingBlocks

Ocow.Contracts
  可依赖 Ocow.Shared

Ocow.AspNetCore
  可依赖 Ocow.Shared、Ocow.Auth、Ocow.Observability

Ocow.Auth
  可依赖 Ocow.Shared

Ocow.InternalAuth
  可依赖 Ocow.Shared

Ocow.EntityFrameworkCore
  可依赖 Ocow.Shared

Ocow.Redis
  可依赖 Ocow.Shared

Ocow.MessageBus
  可依赖 Ocow.Shared、Ocow.Contracts、Ocow.Observability

Ocow.Observability
  可依赖 Ocow.Shared

Ocow.HealthChecks
  可依赖 Ocow.Shared

Ocow.ServiceDiscovery
  可依赖 Ocow.Shared、Ocow.HealthChecks
```


---

# 8. 最终推荐项目清单

| 分组 | 项目名 | 是否推荐 | 说明 |
| --- | --- | --- | --- |
| 分组 | 项目名 | 是否推荐 | 说明 |
| BuildingBlocks | `Ocow.Shared` | 必须 | 公共基础能力 |
| BuildingBlocks | `Ocow.Contracts` | 必须 | 跨服务契约 |
| BuildingBlocks | `Ocow.AspNetCore` | 建议 | Web API 通用封装 |
| BuildingBlocks | `Ocow.Auth` | 建议 | 用户 JWT 与权限 |
| BuildingBlocks | `Ocow.InternalAuth` | 建议 | 服务间认证 |
| BuildingBlocks | `Ocow.EntityFrameworkCore` | 必须 | EF Core 通用封装 |
| BuildingBlocks | `Ocow.Redis` | 建议 | Redis、缓存、锁、限流、幂等 |
| BuildingBlocks | `Ocow.MessageBus` | 建议 | RabbitMQ、集成事件 |
| BuildingBlocks | `Ocow.Observability` | 建议 | 日志、Trace、Metrics |
| BuildingBlocks | `Ocow.HealthChecks` | 建议 | 健康检查 |
| BuildingBlocks | `Ocow.ServiceDiscovery` | 可选 | Consul / Nacos / K8S DNS 适配 |
| Gateways | `Ocow.Gateway` | 建议 | Ocelot 网关 |
| Services | `Ocow.Identity.*` | 必须 | 身份认证服务 |
| Services | `Ocow.Member.*` | 建议 | 会员服务 |
| Services | `Ocow.Product.*` | 必须 | 商品服务 |
| Services | `Ocow.Inventory.*` | 建议 | 库存服务 |
| Services | `Ocow.Order.*` | 必须 | 订单服务 |
| Services | `Ocow.Payment.*` | 必须 | 支付服务 |
| Services | `Ocow.WeChat.*` | 建议 | 微信集成服务 |
| Services | `Ocow.Scheduler.*` | 建议 | 定时任务服务 |
| tests | `Ocow.Tests.Unit` | 建议 | 单元测试 |
| tests | `Ocow.Tests.Integration` | 建议 | 集成测试 |

---

## 简化版结论

你这个文档可以改成：

``` text
BuildingBlocks：放跨服务公共能力
Gateways：放统一入口网关
Services：放具体业务微服务
tests：放测试项目
```


当前可以保留但标为可选：

``` text
Ocow.ServiceDiscovery
```

这样项目边界会比原来更清晰，后面业务服务增多也不容易乱。

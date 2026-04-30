# Ocow 电商骨架实施计划

## 1. 实施目标

下一步按「多人多服务开发」方式搭建 Ocow .NET 8 电商骨架。第一轮目标不是一次性完成所有业务，而是先搭出可编译、可启动、可联调、边界清晰的最小骨架。

网关统一使用 `Ocelot`，不使用 YARP。

默认实施顺序：

1. 基础骨架
2. 订单服务 MVP
3. 网关联调
4. 登录认证、权限管理与内部调用
5. Redis 与幂等
6. RabbitMQ 消息
7. 定时任务
8. 微信集成
9. 日志和追踪

## 2. 第一阶段：基础骨架

目标：

- 创建解决方案 `Ocow.sln`。
- 创建 `Ocow.Gateway`，集成 Ocelot。
- 创建 `Ocow.Shared`、`Ocow.Contracts`。
- 创建 `Ocow.Redis`、`Ocow.MessageBus`、`Ocow.InternalAuth`、`Ocow.EntityFrameworkCore`、`Ocow.Identity.*` 基础项目。
- 建立统一命名、DTO、Options、Models、Enums 目录规范。
- 建立 EF Core 通用封装和多数据库 Provider 选择规范。
- 建立 EF Core 通用 Seeder 抽象和业务种子数据放置规范。
- 建立 `*.Migrations` 可执行 Console 项目模板，统一提供 `Program.cs` 迁移和种子执行入口。
- 建立 REST API 服务统一 Swagger / OpenAPI 规范。

建议项目：

```text
Ocow.sln
src
  Gateways
    Ocow.Gateway
  BuildingBlocks
    Ocow.Shared
    Ocow.Contracts
    Ocow.Redis
    Ocow.MessageBus
    Ocow.InternalAuth
    Ocow.EntityFrameworkCore
  Services
    Identity
      Ocow.Identity.Api
      Ocow.Identity.Application
      Ocow.Identity.Domain
      Ocow.Identity.Infrastructure
      Ocow.Identity.Migrations
```

验收标准：

- 解决方案可以编译。
- `Ocow.Gateway` 可以启动。
- Ocelot 配置文件存在。
- 基础项目引用关系清晰。
- `Ocow.EntityFrameworkCore` 已创建，包含 EF Core 通用基础接口、拦截器、Provider 配置扩展、通用 Seeder 抽象。
- `Ocow.Identity.*` 项目已创建，职责边界不和 `Ocow.Member`、`Ocow.WeChat` 混淆。
- `*.Migrations` 模板明确为 Console 可执行项目，`OutputType=Exe`，`TargetFramework=net8.0`。
- `*.Migrations/Program.cs` 支持 `migrate`、`seed`、`migrate-seed` 命令。
- 公共目录规范已确定。
- Swagger 分组规范已确定，包含 `Client`、`Admin`、`Internal`、`Notify`。

## 3. 第二阶段：订单服务 MVP

目标：

- 创建 `Ocow.Order.Api`。
- 创建 `Ocow.Order.Application`。
- 创建 `Ocow.Order.Domain`。
- 创建 `Ocow.Order.Infrastructure`。
- 创建 `Ocow.Order.Migrations`。
- 添加小程序订单 Controller、后台订单 Controller、内部同步 Controller。
- 添加订单状态枚举、订单基础模型、订单应用服务接口。
- 接入 EF Core，默认使用 PostgreSQL，并预留 MySQL、SQL Server Provider 切换能力。
- 接入 Swagger / OpenAPI，并按 `Client`、`Admin`、`Internal` 分组。

订单服务目录建议：

```text
Ocow.Order.Api
  Controllers
    Client
      OrdersController.cs
    Admin
      AdminOrdersController.cs
    Internal
      InternalOrderSyncController.cs

Ocow.Order.Application
  Dtos
  Services

Ocow.Order.Domain
  Models
    Order.cs
    OrderItem.cs
  Enums
  Services

Ocow.Order.Infrastructure
  Persistence
    OrderDbContext.cs
    Configurations
      OrderEntityTypeConfiguration.cs
      OrderItemEntityTypeConfiguration.cs
  Repositories
  Options

Ocow.Order.Migrations
  Program.cs
  appsettings.json
  Migrations
  DesignTime
    OrderDbContextFactory.cs
  Seeders
    OrderStatusSeeder.cs
    OrderSeedRunner.cs
```

路由：

```text
/api/orders/**
/api/admin/orders/**
/internal/orders/**
```

验收标准：

- 订单服务可以启动。
- Swagger 可以查看订单接口，并能区分 `Client`、`Admin`、`Internal` 分组。
- EF Core 迁移项目可以生成迁移。
- `Ocow.Order.Migrations` 是 Console 可执行项目，入口是 `Program.cs`。
- `dotnet run --project src/Services/Order/Ocow.Order.Migrations -- migrate-seed` 可以作为订单数据库初始化命令。
- 业务实体位于 `Ocow.Order.Domain/Models`，DbContext 和实体映射位于 `Ocow.Order.Infrastructure`。
- `Ocow.Order.Infrastructure` 通过 `Ocow.EntityFrameworkCore` 选择 PostgreSQL、MySQL 或 SQL Server Provider。
- 订单服务种子数据位于 `Ocow.Order.Migrations/Seeders`，不放在 `Api`、`Application`、`Domain`、`Infrastructure`。
- 小程序、后台、内部接口路由隔离清楚。

## 4. 第三阶段：网关联调

目标：

- 配置 Ocelot 静态路由。
- 将 `/api/orders/**` 转发到订单服务。
- 将 `/api/admin/orders/**` 转发到订单服务。
- 将 `/internal/orders/**` 转发到订单服务。
- 增加请求 TraceId 中间件。
- MVP 阶段各服务自己暴露 Swagger，Gateway 暂不聚合 Swagger。
- 第二阶段预留 Gateway 聚合各服务 Swagger 的方案。

本地路由示例：

```text
Ocow.Gateway
  /api/orders/**          -> http://localhost:5101
  /api/admin/orders/**    -> http://localhost:5101
  /internal/orders/**     -> http://localhost:5101
```

验收标准：

- 通过 Gateway 可以访问订单服务小程序接口。
- 通过 Gateway 可以访问订单服务后台接口。
- 通过 Gateway 可以访问订单服务内部接口。
- 日志中可以看到同一个请求的 `TraceId`。
- 订单服务自身 Swagger 可以独立访问。

## 5. 第四阶段：登录认证、权限管理与内部调用

目标：

- 实现 `Ocow.Identity` 身份认证服务基础骨架。
- 实现小程序登录入口。
- 实现后台管理员登录入口。
- 实现管理员、角色、权限点管理基础接口。
- 在 `Ocow.Identity.Migrations/Seeders` 增加权限点、默认角色、默认管理员种子数据。
- 实现 `Customer JWT` 基础校验策略。
- 实现 `Admin JWT` 基础校验策略。
- 实现 `Ocow.InternalAuth` 的 `Service JWT` 校验。
- 保护 `/internal/**` 内部接口。
- 高风险内部接口预留 HMAC 签名能力。

`Ocow.Identity.Api` Controller 设计：

```text
Ocow.Identity.Api
  Controllers
    Client
      ClientAuthController.cs
      ClientProfileController.cs
    Admin
      AdminAuthController.cs
      AdminUsersController.cs
      AdminRolesController.cs
      AdminPermissionsController.cs
```

登录和权限路由：

```text
POST /api/auth/wechat-login
POST /api/auth/refresh-token
POST /api/auth/logout
GET  /api/auth/me

POST /api/admin/auth/login
POST /api/admin/auth/refresh-token
POST /api/admin/auth/logout

GET  /api/admin/users
POST /api/admin/users
PUT  /api/admin/users/{id}
POST /api/admin/users/{id}/disable

GET  /api/admin/roles
POST /api/admin/roles
PUT  /api/admin/roles/{id}

GET  /api/admin/permissions
PUT  /api/admin/roles/{id}/permissions
```

接口权限规则：

```text
/api/auth/**      小程序登录认证入口
/api/**          Customer JWT
/api/admin/auth/** 后台登录认证入口
/api/admin/**    Admin JWT + 权限点
/internal/**     Service JWT
```

权限模型：

```text
管理员 -> 角色 -> 权限点
```

权限点 MVP：

```text
order.read
order.ship
order.close
product.create
product.update
product.publish
payment.refund
scheduler.trigger
```

职责边界：

```text
Ocow.Identity 负责登录、Token、管理员、角色、权限
Ocow.Member 负责会员资料、地址、等级、积分
Ocow.WeChat 负责微信官方接口调用
```

Identity 种子数据目录：

```text
Ocow.Identity.Migrations
  Program.cs
  appsettings.json
  Migrations
  DesignTime
    IdentityDbContextFactory.cs
  Seeders
    IdentityPermissionSeeder.cs
    IdentityRoleSeeder.cs
    IdentityAdminUserSeeder.cs
    IdentitySeedRunner.cs
```

种子数据安全要求：

```text
默认管理员账号可以配置默认值
默认管理员密码不能硬编码
默认管理员密码必须从环境变量、安全配置或密钥管理服务读取
权限点、角色、管理员种子数据必须幂等
```

迁移和种子执行命令：

```bash
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations -- migrate
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations -- seed
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations -- migrate-seed
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations -- migrate-seed --environment Production
```

内部服务调用示例：

```text
Ocow.Scheduler -> POST /internal/orders/sync/erp -> Ocow.Order.Api
```

验收标准：

- 小程序登录成功后可以签发 `Customer JWT`。
- 后台管理员登录成功后可以签发 `Admin JWT`。
- 后台管理员可以绑定角色，角色可以绑定权限点。
- 后台订单发货接口可以校验 `order.ship` 权限。
- `Ocow.Identity.Migrations/Seeders` 可以初始化权限点、默认角色、默认管理员。
- `Ocow.Identity.Migrations/Program.cs` 可以执行 `migrate`、`seed`、`migrate-seed`。
- 默认管理员密码未硬编码到代码或文档配置样例中。
- 用户 Token 不能访问内部接口。
- 管理员 Token 不能访问内部接口。
- Service Token 可以访问授权内部接口。
- 未授权服务不能调用订单同步接口。

## 6. 第五阶段：Redis 与幂等

目标：

- 实现 Redis 配置。
- 实现缓存服务。
- 实现分布式锁服务。
- 实现限流基础能力。
- 订单下单增加幂等 Key 设计。
- 支付回调和定时任务预留幂等能力。

Redis Key 规范：

```text
ocow:{service}:{module}:{business}:{id}
```

重点场景：

```text
ocow:order:idempotent:create:{userId}:{requestId}
ocow:payment:notify:idempotent:{transactionId}
ocow:scheduler:lock:order-erp-sync
```

验收标准：

- Redis 可以连接。
- 缓存写入、读取、删除正常。
- 分布式锁可以加锁和释放。
- 重复下单请求可以被识别。

## 7. 第六阶段：RabbitMQ 消息

目标：

- 实现消息发布订阅基础封装。
- 定义订单创建事件。
- 定义支付成功事件。
- 定义订单发货事件。
- 消息中携带 `TraceId`。

事件建议：

```text
OrderCreated
PaymentSucceeded
OrderShipped
OrderCanceled
```

验收标准：

- 可以发布一条订单事件。
- 可以消费一条订单事件。
- 消费端可以读取 `TraceId`。
- 消费端预留幂等处理。

## 8. 第七阶段：定时任务

目标：

- 创建 `Ocow.Scheduler`。
- 集成 Hangfire。
- 配置每天 0 点调用订单内部同步接口。
- 支持任务启用、禁用、手动触发、执行日志。

同步接口：

```text
POST /internal/orders/sync/erp
```

任务示例：

```text
任务编码：order-erp-sync
任务名称：订单 ERP 同步任务
Cron：0 0 * * *
目标服务：Ocow.Order.Api
目标接口：/internal/orders/sync/erp
```

验收标准：

- Hangfire Dashboard 可以访问。
- 任务可以手动触发。
- 任务可以按 Cron 执行。
- 执行记录可以查询。
- 多实例部署时同一个任务不会重复执行。

## 9. 第八阶段：微信集成

目标：

- 创建 `Ocow.WeChat` 服务。
- 实现小程序登录。
- 实现手机号获取。
- 实现订阅消息 MVP。
- access_token 使用 Redis 缓存和分布式锁。
- 微信接口调用失败记录错误码。

微信普通接口放 `Ocow.WeChat`：

```text
code2Session
获取手机号
订阅消息
公众号 access_token
公众号模板消息
微信回调验签
```

微信支付仍放 `Ocow.Payment`：

```text
微信支付下单
微信支付回调
退款
退款回调
```

和身份认证服务的关系：

```text
Ocow.Identity 负责小程序登录流程和 Customer JWT 签发
Ocow.WeChat 负责调用微信 code2Session、获取手机号等官方接口
Ocow.Member 负责保存会员业务资料
```

小程序登录链路：

```text
微信小程序
  -> Ocow.Gateway
  -> Ocow.Identity.Api
  -> Ocow.WeChat.Api code2Session
  -> Ocow.Member 创建或更新会员资料
  -> Ocow.Identity 签发 Customer JWT
```

验收标准：

- 小程序登录可以通过 `Ocow.Identity` 换取 openid 并签发 `Customer JWT`。
- 重复请求不会频繁刷新微信 access_token。
- 并发刷新 access_token 时只有一个请求真正访问微信。
- 订阅消息发送失败时可以记录微信错误码。
- 微信密钥不会输出到日志。

## 10. 第九阶段：日志和追踪

目标：

- 接入 Serilog。
- 接入 OpenTelemetry。
- 后台高风险操作写审计日志。
- RabbitMQ 消息携带并传递 `TraceId`。

审计日志覆盖：

- 后台发货。
- 后台关闭订单。
- 修改商品价格。
- 商品上下架。
- 退款操作。
- 手动触发定时任务。
- 修改定时任务 Cron。

验收标准：

- 一次 Gateway 到订单服务的请求可以通过 `TraceId` 串起来。
- 异步消息消费日志可以追踪到原始请求。
- 后台高风险操作有审计记录。
- 日志中不输出密码、Token、微信密钥等敏感信息。

## 11. 总体验收标准

- 每个服务可以独立启动。
- Gateway 能正确路由到目标服务。
- 所有 `*.Api` 服务必须接入 Swagger / OpenAPI。
- Swagger 必须按 `Client`、`Admin`、`Internal`、`Notify` 分组。
- 开发环境默认开启 Swagger，生产环境默认关闭或仅内网/VPN 访问。
- 小程序接口、后台接口、内部接口权限隔离清楚。
- 管理员登录、角色管理、权限点管理具备 MVP 能力。
- 小程序登录能完成微信授权、会员绑定和 `Customer JWT` 签发。
- 下单、支付回调、定时任务、微信接口具备幂等或限流设计。
- 高风险后台操作具备审计日志。
- Redis、RabbitMQ、Hangfire、Ocelot 均有最小可运行验证。
- EF Core 通用封装位于 `Ocow.EntityFrameworkCore`，各服务的 DbContext 和迁移项目保持独立。
- `Ocow.EntityFrameworkCore` 只放通用播种机制，不放具体业务种子数据。
- 具体业务 Seeder 必须位于对应服务的 `*.Migrations/Seeders`。
- `*.Migrations` 项目必须是 Console 可执行项目，入口是 `Program.cs`。
- `*.Migrations/Program.cs` 必须支持 `migrate`、`seed`、`migrate-seed` 三个命令。
- `dotnet run --project src/Services/Identity/Ocow.Identity.Migrations -- migrate-seed` 可以执行。
- `Api` 项目启动时不自动迁移、不自动播种。
- `Api` 项目不引用对应服务的 `*.Migrations` 项目。
- Identity 的权限点、默认角色、默认管理员种子数据位于 `Ocow.Identity.Migrations/Seeders`。
- 默认管理员密码不能硬编码。
- 至少验证 PostgreSQL Provider 可用，并保留 MySQL、SQL Server Provider 配置入口。
- 代码目录符合 DTO、Options、Models、Enums 命名约定。
- 新增方法和接口均有中文注释说明作用。

## 12. 默认假设

- 下一步先按本文档创建骨架代码项目。
- 网关使用 Ocelot。
- 本地第一阶段使用 Ocelot 静态路由，不立即接 Consul。
- MVP 阶段 Swagger 由各服务独立暴露，Gateway 暂不聚合。
- 数据库默认 PostgreSQL，同时预留 MySQL、SQL Server 兼容能力。
- Redis、RabbitMQ、Hangfire 先做公共封装和最小示例，不一次性实现所有业务细节。
- `Ocow.Identity` 负责登录认证和权限管理，`Ocow.Member` 不负责签发 Token。
- 业务实体放各服务 `Domain/Models`，EF Core 运行时实现放各服务 `Infrastructure`，迁移放各服务 `Migrations`，具体业务种子数据放各服务 `Migrations/Seeders`，通用封装和通用 Seeder 抽象放 `Ocow.EntityFrameworkCore`。
- 各服务 `*.Migrations` 是可执行 Console 项目，不是纯类库；迁移和种子数据初始化只能通过 `*.Migrations/Program.cs` 或 CI/CD Job 执行。
- MVP 阶段优先打通订单服务，商品、支付、会员、库存、微信服务按后续阶段逐步补齐。

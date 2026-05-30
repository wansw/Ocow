# Ocow Docker 部署说明

本文档说明两种部署方式：

1. 完整栈一键启动：前端、网关、后端服务、数据库、Redis、RabbitMQ、迁移容器全部由 `docker-compose.full.yml` 管理。
2. 分开启动：先启动后端+网关+迁移，再启动前端 `docker-compose.admin.yml`。

两种方式不要同时启动，因为容器名相同。切换方式前先执行对应的 `docker compose down`。

所有命令都在仓库根目录执行：

```powershell
cd F:\DevCode\AICodex\架构3
```

## 一、部署文件

本次 Docker 部署相关文件如下：

- `.dockerignore`：减少后端 Docker 构建上下文，排除 `bin`、`obj`、`node_modules` 等目录。
- `.env.docker.example`：Docker 环境变量示例，可复制为 `.env` 使用。
- `docker/dotnet/Dockerfile`：后端通用 Dockerfile，通过 `PROJECT_PATH` 构建不同的 API、网关、迁移项目。
- `compose/postgres.yml`：PostgreSQL 单独部署。
- `compose/redis.yml`：Redis 单独部署。
- `compose/rabbitmq.yml`：RabbitMQ 单独部署。
- `compose/migrations.yml`：数据库迁移容器单独部署。
- `compose/apis.yml`：Identity、Order、Jobs 微服务 API 单独部署。
- `compose/gateway.yml`：网关单独部署。
- `compose/admin-web.yml`：后台前端单独部署。
- `compose/README.md`：分拆 compose 的具体启动步骤。
- `docker-compose.full.yml`：完整栈一键启动。
- `docker-compose.backend.yml`：只启动后端基础设施、迁移、API、网关。
- `docker-compose.admin.yml`：只启动后台前端。
- `src/Admin/Ocow.Admin.Web/Dockerfile`：后台前端 Dockerfile。
- `src/Admin/Ocow.Admin.Web/nginx/default.conf.template`：前端 nginx 反向代理配置。

整体调用链：

```text
Browser -> ocow-admin-web -> ocow-gateway -> Identity / Order / Jobs
```

## 二、准备环境变量

可以先复制示例文件：

```powershell
Copy-Item .env.docker.example .env
```

本地开发可以先用默认值。生产环境至少要改这些值：

- `OCOW_POSTGRES_PASSWORD`
- `OCOW_RABBITMQ_PASSWORD`
- `OCOW_IDENTITY_ADMIN_PASSWORD`
- `OCOW_JWT_SECRET`
- `OCOW_GATEWAY_FORWARDED_JWT_SECRET`
- `OCOW_INTERNAL_AUTH_SECRET`
- `OCOW_HMAC_SIGNATURE_SECRET`

默认对外端口：

| 服务 | 宿主机地址 |
| --- | --- |
| 后台前端 | `http://localhost:8088` |
| 网关 | `http://localhost:5000` |
| PostgreSQL | `localhost:15432` |
| Redis | `localhost:16379` |
| RabbitMQ AMQP | `localhost:15670` |
| RabbitMQ 管理页面 | `http://localhost:15672` |

容器内部仍然使用标准端口，例如 PostgreSQL 是 `ocow-postgres:5432`，RabbitMQ 是 `ocow-rabbitmq:5672`。

## 三、方式一：按服务类型分拆启动

这是最接近生产拆分的方式。所有分拆文件都放在 `compose/` 目录，并共享 Docker 网络 `ocow-net`。

### 1. 启动 PostgreSQL、Redis、RabbitMQ

```powershell
docker compose --env-file .env -f compose/postgres.yml up -d
docker compose --env-file .env -f compose/redis.yml up -d
docker compose --env-file .env -f compose/rabbitmq.yml up -d
```

### 2. 执行数据库迁移

```powershell
docker compose --env-file .env -f compose/migrations.yml up --build
```

### 3. 启动微服务 API

```powershell
docker compose --env-file .env -f compose/apis.yml up -d --build
```

### 4. 启动 Gateway

```powershell
docker compose --env-file .env -f compose/gateway.yml up -d --build
```

### 5. 启动前端

```powershell
docker compose --env-file .env -f compose/admin-web.yml up -d --build
```

访问：

```text
http://localhost:8088
```

更详细的分拆命令见 `compose/README.md`。

## 四、方式二：完整栈一键启动

这一步会启动所有容器，并自动执行三个迁移容器：

```powershell
docker compose -f docker-compose.full.yml up -d --build
```

查看容器状态：

```powershell
docker compose -f docker-compose.full.yml ps
```

迁移容器正常完成后状态会是 `Exited (0)`，API 和网关会继续运行。查看迁移日志：

```powershell
docker compose -f docker-compose.full.yml logs identity-migrations
docker compose -f docker-compose.full.yml logs order-migrations
docker compose -f docker-compose.full.yml logs jobs-migrations
```

访问后台：

```text
http://localhost:8088
```

默认账号：

```text
admin / Admin@123456
```

在完整栈模式下，前端 nginx 代理到容器网络里的 `http://ocow-gateway:8080`，不需要你手动执行 `$env:GATEWAY_BASE_URL=...`。

## 五、方式三：后端+网关、迁移、前端分开启动

### 1. 启动后端+网关+迁移

```powershell
docker compose -f docker-compose.backend.yml up -d --build
```

确认状态：

```powershell
docker compose -f docker-compose.backend.yml ps
```

查看网关日志：

```powershell
docker compose -f docker-compose.backend.yml logs -f ocow-gateway
```

### 2. 启动前端

前端单独启动时，默认会把 `/api` 和 `/hangfire` 代理到 `http://host.docker.internal:5000`，也就是宿主机暴露出来的网关端口。

```powershell
docker compose -f docker-compose.admin.yml up -d --build
```

如果你的网关不是 `localhost:5000`，先设置代理地址：

```powershell
$env:GATEWAY_BASE_URL="http://host.docker.internal:5000"
docker compose -f docker-compose.admin.yml up -d --build
Remove-Item Env:GATEWAY_BASE_URL
```

然后访问：

```text
http://localhost:8088
```

## 六、迁移容器说明

迁移容器是一次性任务：

- `identity-migrations`：迁移 Identity 数据库，并写入后台管理员、角色、菜单、权限种子数据。
- `order-migrations`：迁移 Order 数据库。
- `jobs-migrations`：迁移 Jobs 数据库。

手动重新执行迁移：

```powershell
docker compose -f docker-compose.full.yml run --rm identity-migrations
docker compose -f docker-compose.full.yml run --rm order-migrations
docker compose -f docker-compose.full.yml run --rm jobs-migrations
```

分开部署时把 compose 文件换成 `docker-compose.backend.yml`：

```powershell
docker compose -f docker-compose.backend.yml run --rm identity-migrations
```

## 七、健康检查

查看 Docker 健康状态：

```powershell
docker compose -f docker-compose.full.yml ps
```

通过网关检查后端服务：

```powershell
Invoke-WebRequest http://localhost:5000/health/identity
Invoke-WebRequest http://localhost:5000/health/order
Invoke-WebRequest http://localhost:5000/health/jobs
```

完整栈里的容器健康检查包括：

- `ocow-postgres`：`pg_isready`
- `ocow-redis`：`redis-cli ping`
- `ocow-rabbitmq`：`rabbitmq-diagnostics ping`
- `ocow-identity-api`、`ocow-order-api`、`ocow-jobs-api`：`/health`
- `ocow-gateway`：`/health/identity`
- `ocow-admin-web`：前端首页

## 八、常用维护命令

查看日志：

```powershell
docker compose -f docker-compose.full.yml logs -f ocow-admin-web
docker compose -f docker-compose.full.yml logs -f ocow-gateway
docker compose -f docker-compose.full.yml logs -f ocow-identity-api
```

只重建某个服务：

```powershell
docker compose -f docker-compose.full.yml up -d --build ocow-identity-api
```

停止容器但保留数据卷：

```powershell
docker compose -f docker-compose.full.yml down
```

停止并删除数据库、Redis、RabbitMQ 数据卷：

```powershell
docker compose -f docker-compose.full.yml down -v
```

## 九、502 排查

登录时报 502，通常是前端 nginx 没有连上 Gateway：

1. 完整栈模式下检查 `ocow-gateway` 是否健康：

```powershell
docker compose -f docker-compose.full.yml ps ocow-gateway
docker compose -f docker-compose.full.yml logs -f ocow-gateway
```

2. 分开启动模式下确认网关宿主机端口可访问：

```powershell
Invoke-WebRequest http://localhost:5000/health/identity
```

3. 分开启动前端时，`GATEWAY_BASE_URL` 应该指向宿主机网关地址：

```text
http://host.docker.internal:5000
```

4. 如果你修改过 `.env` 里的端口，要同步检查浏览器访问地址和 `GATEWAY_BASE_URL`。

## 十、生产环境注意事项

生产环境建议：

- 不要使用示例密钥和默认密码。
- 不要把 PostgreSQL、Redis、RabbitMQ 直接暴露到公网。
- 网关和前端前面可以放公司统一的 nginx、SLB 或云负载均衡。
- 数据库、Redis、RabbitMQ 要配置持久化、备份和监控。
- 发布前先执行 `docker compose -f docker-compose.full.yml config` 检查配置是否能正确展开。

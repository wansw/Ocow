# Compose 分拆部署说明

这些文件都使用同一个项目名 `ocow`、同一个网络 `ocow-net`，可以分步启动，也可以按需单独重启某一类服务。

## 1. 准备环境变量

在仓库根目录执行：

```powershell
Copy-Item .env.docker.example .env
```

后续命令建议都带上 `--env-file .env`，避免因为 compose 文件在 `compose/` 子目录导致环境变量来源不清楚。

## 2. 启动基础设施

```powershell
docker compose --env-file .env -f compose/postgres.yml up -d
docker compose --env-file .env -f compose/redis.yml up -d
docker compose --env-file .env -f compose/rabbitmq.yml up -d
```

查看状态：

```powershell
docker compose --env-file .env -f compose/postgres.yml ps
docker compose --env-file .env -f compose/redis.yml ps
docker compose --env-file .env -f compose/rabbitmq.yml ps
```

## 3. 执行迁移

```powershell
docker compose --env-file .env -f compose/migrations.yml up --build
```

迁移容器是一次性容器，成功后会退出。需要重新执行时：

```powershell
docker compose --env-file .env -f compose/migrations.yml up --build --force-recreate
```

## 4. 启动微服务 API

```powershell
docker compose --env-file .env -f compose/apis.yml up -d --build
```

## 5. 启动网关

```powershell
docker compose --env-file .env -f compose/gateway.yml up -d --build
```

检查网关：

```powershell
Invoke-WebRequest http://localhost:5000/health/identity
Invoke-WebRequest http://localhost:5000/health/order
Invoke-WebRequest http://localhost:5000/health/jobs
```

## 6. 启动前端

```powershell
docker compose --env-file .env -f compose/admin-web.yml up -d --build
```

访问：

```text
http://localhost:8088
```

默认后台账号：

```text
admin / Admin@123456
```

## 7. 常用组合命令

只看全部容器：

```powershell
docker ps --filter "name=ocow-"
```

停止前端：

```powershell
docker compose --env-file .env -f compose/admin-web.yml down
```

停止全部分拆服务：

```powershell
docker compose --env-file .env -f compose/admin-web.yml down
docker compose --env-file .env -f compose/gateway.yml down
docker compose --env-file .env -f compose/apis.yml down
docker compose --env-file .env -f compose/migrations.yml down
docker compose --env-file .env -f compose/rabbitmq.yml down
docker compose --env-file .env -f compose/redis.yml down
docker compose --env-file .env -f compose/postgres.yml down
```

停止并删除基础设施数据卷：

```powershell
docker compose --env-file .env -f compose/postgres.yml down -v
docker compose --env-file .env -f compose/redis.yml down -v
docker compose --env-file .env -f compose/rabbitmq.yml down -v
```

## 8. 注意事项

- 分拆文件里不写跨文件 `depends_on`，所以要按本文顺序启动。
- 前端默认代理到 Docker 网络内的 `http://ocow-gateway:8080`。
- 如果只单独启动前端，并让它访问宿主机网关，可以覆盖：

```powershell
$env:GATEWAY_BASE_URL="http://host.docker.internal:5000"
docker compose --env-file .env -f compose/admin-web.yml up -d --build
Remove-Item Env:GATEWAY_BASE_URL
```

# Ocow Admin Web Docker 部署说明

## 运行方式

后台前端以 nginx 容器托管，浏览器访问前端容器，接口统一从前端 nginx 反向代理到 Gateway。

```text
Browser -> ocow-admin-web -> Ocow.Gateway -> Identity / Order / Jobs
```

本机 Gateway 使用 `http://localhost:5000` 运行时：

```bash
docker compose -f docker-compose.admin.yml up -d --build
```

访问：

```text
http://localhost:8088
```

Gateway 已经在 Docker 网络中运行时，覆盖 `GATEWAY_BASE_URL`：

```bash
GATEWAY_BASE_URL=http://ocow-gateway:8080 docker compose -f docker-compose.admin.yml up -d --build
```

## 默认账号

后端种子数据默认账号以 Identity 服务配置为准，当前前端登录页默认填入：

```text
admin / Admin@123456
```

## 代理路径

前端 nginx 会代理：

- `/api/**` -> Gateway
- `/hangfire/**` -> Gateway

Hangfire Dashboard 仍由后端 Dashboard 授权控制。

当前 `Identity.Migrations` 的命令不是 `migrate-seed`，而是：

```bash
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations/Ocow.Identity.Migrations.csproj -- init
```

这个命令会执行：

```text
1. 初始化数据库结构
   - 如果存在 EF Migrations，则执行 MigrateAsync()
   - 如果没有 Migrations，则执行 EnsureCreatedAsync()

2. 执行种子数据
   - 权限点
   - 默认角色
   - 默认管理员
```

如果只初始化表结构，不执行种子数据：

```bash
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations/Ocow.Identity.Migrations.csproj -- init --no-seed
```

如果要指定默认管理员密码，先设置环境变量：

```powershell
$env:OCOW_IDENTITY_ADMIN_PASSWORD="Admin@123456"
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations/Ocow.Identity.Migrations.csproj -- init
```

可选环境变量：

```powershell
$env:OCOW_IDENTITY_DB_PROVIDER="PostgreSql"
$env:OCOW_IDENTITY_DB_CONNECTION_STRING="Host=localhost;Port=5432;Database=ocow_identity;Username=postgres;Password=postgres123"
$env:OCOW_IDENTITY_ADMIN_USERNAME="admin"
$env:OCOW_IDENTITY_ADMIN_DISPLAY_NAME="系统管理员"
$env:OCOW_IDENTITY_ADMIN_PASSWORD="Admin@123456"
```

最推荐你本地直接执行这一组：

```powershell
$env:OCOW_IDENTITY_ADMIN_PASSWORD="Admin@123456"
dotnet run --project src/Services/Identity/Ocow.Identity.Migrations/Ocow.Identity.Migrations.csproj -- init
```

powershell和cmd命令窗口不一样

```powershell
$env:OCOW_IDENTITY_ADMIN_PASSWORD="Admin@123456"
```

```cmd
set OCOW_IDENTITY_ADMIN_PASSWORD=Admin@123456
```
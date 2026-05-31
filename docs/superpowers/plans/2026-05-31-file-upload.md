# File Upload Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 新增独立 `Ocow.File` 文件服务，完成本地文件上传、文件安全校验和元数据入库能力。

**Architecture:** 新服务按现有 `Api / Application / Domain / Infrastructure / Migrations` 分层。第一版只实现上传闭环，本地存储通过 `IFileStorageProvider` 抽象，后续 COS 通过新增 Provider 接入。

**Tech Stack:** .NET 8、ASP.NET Core Web API、EF Core、PostgreSQL、xUnit。

---

### Task 1: 项目骨架

**Files:**
- Create: `src/Services/File/Ocow.File.Api/Ocow.File.Api.csproj`
- Create: `src/Services/File/Ocow.File.Application/Ocow.File.Application.csproj`
- Create: `src/Services/File/Ocow.File.Domain/Ocow.File.Domain.csproj`
- Create: `src/Services/File/Ocow.File.Infrastructure/Ocow.File.Infrastructure.csproj`
- Create: `src/Services/File/Ocow.File.Migrations/Ocow.File.Migrations.csproj`
- Modify: `Ocow.sln`
- Modify: `tests/Ocow.Tests.Unit/Ocow.Tests.Unit.csproj`

- [ ] 创建五个项目并加入解决方案。
- [ ] 为测试项目添加 `Ocow.File.Application`、`Ocow.File.Domain`、`Ocow.File.Infrastructure` 引用。

### Task 2: 文件校验

**Files:**
- Create: `tests/Ocow.Tests.Unit/FileUploadTests.cs`
- Create: `src/Services/File/Ocow.File.Application/Options/FileUploadOption.cs`
- Create: `src/Services/File/Ocow.File.Application/Interfaces/IFileValidator.cs`
- Create: `src/Services/File/Ocow.File.Application/Models/FileValidationContext.cs`
- Create: `src/Services/File/Ocow.File.Application/Models/FileValidationResult.cs`
- Create: `src/Services/File/Ocow.File.Infrastructure/Validation/FileValidator.cs`

- [ ] 写失败测试：危险扩展名 `.exe` 被拒绝。
- [ ] 实现最小校验逻辑：扩展名、大小、文件头。
- [ ] 跑测试确认通过。

### Task 3: 本地存储

**Files:**
- Modify: `tests/Ocow.Tests.Unit/FileUploadTests.cs`
- Create: `src/Services/File/Ocow.File.Application/Interfaces/IFileStorageProvider.cs`
- Create: `src/Services/File/Ocow.File.Application/Models/FileStorageSaveContext.cs`
- Create: `src/Services/File/Ocow.File.Application/Models/FileStorageSaveResult.cs`
- Create: `src/Services/File/Ocow.File.Infrastructure/Storage/LocalFileStorageProvider.cs`

- [ ] 写失败测试：保存文件时生成 `yyyy/MM/dd/{guid}.ext` object_key。
- [ ] 实现本地存储 Provider。
- [ ] 跑测试确认通过。

### Task 4: 上传应用服务和 API

**Files:**
- Modify: `tests/Ocow.Tests.Unit/FileUploadTests.cs`
- Create: `src/Services/File/Ocow.File.Domain/Models/FileResource.cs`
- Create: `src/Services/File/Ocow.File.Domain/Enums/FileCategoryEnum.cs`
- Create: `src/Services/File/Ocow.File.Domain/Enums/FileResourceStatusEnum.cs`
- Create: `src/Services/File/Ocow.File.Domain/Enums/FileStorageTypeEnum.cs`
- Create: `src/Services/File/Ocow.File.Application/Dtos/UploadFileResDto.cs`
- Create: `src/Services/File/Ocow.File.Application/Interfaces/IFileResourceRepository.cs`
- Create: `src/Services/File/Ocow.File.Application/Interfaces/IFileUploadAppService.cs`
- Create: `src/Services/File/Ocow.File.Application/Services/FileUploadAppService.cs`
- Create: `src/Services/File/Ocow.File.Infrastructure/Data/FileDbContext.cs`
- Create: `src/Services/File/Ocow.File.Infrastructure/Repositories/FileResourceRepository.cs`
- Create: `src/Services/File/Ocow.File.Infrastructure/Extensions/FileInfrastructureServiceCollectionExtensions.cs`
- Create: `src/Services/File/Ocow.File.Application/Extensions/FileApplicationServiceCollectionExtensions.cs`
- Create: `src/Services/File/Ocow.File.Api/Controllers/Admin/AdminFilesController.cs`
- Create: `src/Services/File/Ocow.File.Api/Controllers/Client/FilesController.cs`
- Create: `src/Services/File/Ocow.File.Api/Program.cs`

- [ ] 写失败测试：上传服务保存文件并写入元数据。
- [ ] 实现应用服务、仓储、DbContext、Controller。
- [ ] 跑测试和 `dotnet build`。

### Task 5: 迁移和网关

**Files:**
- Create: `src/Services/File/Ocow.File.Migrations/Program.cs`
- Create: `src/Services/File/Ocow.File.Migrations/Factories/FileDbContextFactory.cs`
- Create: `src/Services/File/Ocow.File.Api/appsettings.json`
- Create: `src/Services/File/Ocow.File.Api/appsettings.Development.json`
- Create: `src/Services/File/Ocow.File.Api/appsettings.Production.json`
- Modify: `src/Gateways/Ocow.Gateway/ocelot.json`
- Modify: `src/Gateways/Ocow.Gateway/ocelot.Docker.json`

- [ ] 增加迁移项目入口和设计时工厂。
- [ ] 增加 File 服务配置。
- [ ] 增加网关路由。
- [ ] 运行最终验证。

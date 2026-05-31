using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Options;
using Ocow.Files.Infrastructure.Data;
using Ocow.Files.Infrastructure.Repositories;
using Ocow.Files.Infrastructure.Storage;
using Ocow.Files.Infrastructure.Validation;

namespace Ocow.Files.Infrastructure.Extensions;

/// <summary>
/// 文件基础设施层服务注册扩展。
/// </summary>
public static class FileInfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// 注册文件数据库上下文、仓储、校验器和存储提供者。
    /// </summary>
    public static IServiceCollection AddFileInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileUploadOption>(configuration.GetSection(FileUploadOption.SectionName).Bind);
        services.Configure<FileStorageOption>(configuration.GetSection(FileStorageOption.SectionName).Bind);
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IFileValidator, FileValidator>();
        services.AddScoped<LocalFileStorageProvider>();
        services.AddScoped<ITencentCosClient, TencentCosClient>();
        services.AddScoped<TencentCosFileStorageProvider>();
        services.AddScoped<FileStorageProviderFactory>();
        services.AddScoped<IFileStorageProvider>(provider => provider.GetRequiredService<FileStorageProviderFactory>().Create());
        services.AddOcowDbContext<FileDbContext>(configuration, repos =>
        {
            repos.AddScoped<IFileResourceRepository, FileResourceRepository>();
        });

        return services;
    }
}

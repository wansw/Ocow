using Microsoft.Extensions.DependencyInjection;
using Ocow.Files.Application.Interfaces;
using Ocow.Files.Application.Services;

namespace Ocow.Files.Application.Extensions;

/// <summary>
/// 文件应用层服务注册扩展。
/// </summary>
public static class FileApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 注册文件上传应用服务。
    /// </summary>
    public static IServiceCollection AddFileApplication(this IServiceCollection services)
    {
        services.AddScoped<IFileUploadAppService, FileUploadAppService>();
        return services;
    }
}

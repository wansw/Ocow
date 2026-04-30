using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Ocow.Shared.OpenApi;
using Ocow.Shared.Options;

namespace Ocow.Shared.Extensions;

/// <summary>
/// OpenAPI 服务注册扩展。
/// </summary>
public static class OpenApiServiceCollectionExtensions
{
    private static readonly string[] GroupNames =
    {
        OpenApiGroupNames.Client,
        OpenApiGroupNames.Admin,
        OpenApiGroupNames.Internal,
        OpenApiGroupNames.Notify
    };

    /// <summary>
    /// 注册 Ocow 统一 Swagger / OpenAPI 配置。
    /// </summary>
    public static IServiceCollection AddOcowOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        var option = configuration.GetSection("OpenApi").Get<OpenApiOption>() ?? new OpenApiOption();
        services.Configure<OpenApiOption>(configuration.GetSection("OpenApi"));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            foreach (var groupName in GroupNames)
            {
                options.SwaggerDoc(groupName, new OpenApiInfo
                {
                    Title = $"{option.ServiceName} - {groupName}",
                    Version = option.Version,
                    Description = option.Description
                });
            }

            options.DocInclusionPredicate((documentName, apiDescription) =>
            {
                var groupName = ResolveGroupName(apiDescription.RelativePath ?? string.Empty);
                return string.Equals(documentName, groupName, StringComparison.OrdinalIgnoreCase);
            });

            options.TagActionsBy(apiDescription => new[] { ResolveGroupName(apiDescription.RelativePath ?? string.Empty) });
            options.OperationFilter<SwaggerDefaultValues>();
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "请输入 JWT Token。格式：Bearer {token}"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
        });

        return services;
    }

    /// <summary>
    /// 根据接口路径解析 OpenAPI 分组。
    /// </summary>
    private static string ResolveGroupName(string relativePath)
    {
        var path = relativePath.TrimStart('/').ToLowerInvariant();
        if (path.Contains("notify"))
        {
            return OpenApiGroupNames.Notify;
        }

        if (path.StartsWith("internal/"))
        {
            return OpenApiGroupNames.Internal;
        }

        if (path.StartsWith("api/admin/"))
        {
            return OpenApiGroupNames.Admin;
        }

        return OpenApiGroupNames.Client;
    }
}

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Http;
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
    //以后改成在配置中
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
        // 从配置中获取 OpenAPI 选项，如果没有配置则使用默认值。
        var option = configuration.GetSection("OpenApi").Get<OpenApiOption>() ?? new OpenApiOption();
        // 将 OpenAPI 选项绑定到 DI 容器中，供后续使用。
        services.Configure<OpenApiOption>(configuration.GetSection("OpenApi"));
        // 注册 API Explorer，支持 Swagger 生成器发现 API 描述。
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // 为每个分组注册一个 Swagger 文档，文档信息从配置中获取。
            foreach (var groupName in GroupNames)
            {
                options.SwaggerDoc(groupName, new OpenApiInfo
                {
                    Title = $"{option.ServiceName} - {groupName}",
                    Version = option.Version,
                    Description = option.Description
                });
            }
            // 配置 Swagger 生成器根据 API 描述的相对路径将接口分配到对应的文档分组中。
            options.DocInclusionPredicate((documentName, apiDescription) =>
            {
                var groupName = ResolveGroupName(apiDescription.RelativePath ?? string.Empty);
                return string.Equals(documentName, groupName, StringComparison.OrdinalIgnoreCase);
            });

            // 配置 Swagger 生成器根据 Controller 或 Action 上的 Tags 特性将接口分配到 Swagger UI 内部的业务分类中。
            options.TagActionsBy(apiDescription => new[] { ResolveTagName(apiDescription) });
            // 配置 Swagger 生成器根据 Controller 和 Action 生成稳定的 OperationId，便于前端识别接口。
            options.CustomOperationIds(ResolveOperationId);
            // 添加自定义的 OperationFilter，用于设置 Swagger 文档中的默认值。
            options.OperationFilter<SwaggerDefaultValues>();
            // 配置 Swagger 生成器使用 JWT Bearer 认证方案，并在 Swagger UI 中显示相应的输入框。
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "请输入 JWT Token。格式：Bearer {token}"
            });
            // 配置 Swagger 生成器要求所有接口都必须使用 JWT Bearer 认证方案。
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

            // 配置 Swagger 生成器包含 XML 注释文件，以便在 Swagger UI 中显示接口的注释信息。这里假设 XML 注释文件与程序集位于同一目录下，并且以 .xml 结尾。
            foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
            {
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
            }
        });

        return services;
    }

    /// <summary>
    /// 根据接口路径解析 OpenAPI 文档大分组。
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

    /// <summary>
    /// 根据 Controller 或 Action 上的 Tags 特性解析 Swagger UI 内部业务分类。
    /// </summary>
    private static string ResolveTagName(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return ResolveGroupName(apiDescription.RelativePath ?? string.Empty);
        }

        var tagAttribute = controllerAction.MethodInfo.GetCustomAttributes(typeof(TagsAttribute), true)
            .Concat(controllerAction.ControllerTypeInfo.GetCustomAttributes(typeof(TagsAttribute), true))
            .OfType<TagsAttribute>()
            .FirstOrDefault();

        return tagAttribute?.Tags.FirstOrDefault() ?? controllerAction.ControllerName;
    }

    /// <summary>
    /// 根据 Controller 和 Action 生成稳定的 Swagger OperationId，便于前端识别接口。  这里是导出swagger json的时候给前端用到；去掉该方法也没关系
    /// </summary>
    private static string? ResolveOperationId(ApiDescription apiDescription)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerAction)
        {
            return apiDescription.HttpMethod;
        }

        var actionName = controllerAction.ActionName.EndsWith("Async", StringComparison.Ordinal)
            ? controllerAction.ActionName[..^"Async".Length]
            : controllerAction.ActionName;

        return $"{controllerAction.ControllerName}_{actionName}";
    }
}

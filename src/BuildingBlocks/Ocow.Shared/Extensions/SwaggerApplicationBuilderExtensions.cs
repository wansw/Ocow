using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ocow.Shared.SwaggerApi;

namespace Ocow.Shared.Extensions;

/// <summary>
/// 开发环境启用 Swagger UI
/// </summary>
public static class SwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// 在开发环境启用 Ocow Swagger / OpenAPI UI。
    /// </summary>
    public static WebApplication UseOcowSwagger(this WebApplication app)
    {
        // 仅在开发环境启用 Swagger UI，以避免在生产环境暴露 API 文档。
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();

        // 配置 Swagger UI，添加多个 API 分组的文档端点。
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{SwaggerApiGroupNames.Client}/swagger.json", "Client 小程序用户端接口");
            options.SwaggerEndpoint($"/swagger/{SwaggerApiGroupNames.Admin}/swagger.json", "Admin PC 后台接口");
            options.SwaggerEndpoint($"/swagger/{SwaggerApiGroupNames.Internal}/swagger.json", "Internal 内部服务接口");
            options.SwaggerEndpoint($"/swagger/{SwaggerApiGroupNames.Notify}/swagger.json", "Notify 第三方回调接口");
            options.SwaggerEndpoint($"/swagger/{SwaggerApiGroupNames.Health}/swagger.json", "Health 服务健康检查接口");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}

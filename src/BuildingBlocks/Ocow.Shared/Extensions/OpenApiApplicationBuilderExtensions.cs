using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Ocow.Shared.OpenApi;

namespace Ocow.Shared.Extensions;

/// <summary>
/// OpenAPI 应用管道扩展。
/// </summary>
public static class OpenApiApplicationBuilderExtensions
{
    /// <summary>
    /// 在开发环境启用 Ocow Swagger / OpenAPI UI。
    /// </summary>
    public static WebApplication UseOcowOpenApi(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{OpenApiGroupNames.Client}/swagger.json", "Client 小程序用户端接口");
            options.SwaggerEndpoint($"/swagger/{OpenApiGroupNames.Admin}/swagger.json", "Admin PC 后台接口");
            options.SwaggerEndpoint($"/swagger/{OpenApiGroupNames.Internal}/swagger.json", "Internal 内部服务接口");
            options.SwaggerEndpoint($"/swagger/{OpenApiGroupNames.Notify}/swagger.json", "Notify 第三方回调接口");
            options.SwaggerEndpoint($"/swagger/{OpenApiGroupNames.Health}/swagger.json", "Health 服务健康检查接口");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocow.AspNetCore.SwaggerApi;

namespace Ocow.AspNetCore.Extensions;

/// <summary>
/// Swagger / OpenAPI 应用管道扩展。
/// </summary>
public static class SwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// 在开发环境启用 Ocow Swagger / OpenAPI UI。
    /// </summary>
    public static WebApplication UseOcowSwagger(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
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

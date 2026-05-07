using Ocow.InternalAuth.Extensions;
using Ocow.Order.Application.Extensions;
using Ocow.Order.Infrastructure.Extensions;
using Ocow.Redis.Extensions;
using Ocow.Shared.Extensions;
using Ocow.Shared.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// 注册接口响应服务，统一处理 DTO 参数校验异常返回格式。
builder.Services.AddOcowApiResponse();
// 注册 Ocow OpenAPI 服务，生成接口文档并提供测试界面。
builder.Services.AddOcowOpenApi(builder.Configuration);
// 注册 Redis 连接、缓存、分布式锁和限流服务。
builder.Services.AddOcowRedis(builder.Configuration);
// 注册订单应用服务。
builder.Services.AddOrderApplication();
// 注册订单基础设施服务。
builder.Services.AddOrderInfrastructure(builder.Configuration);
// 注册 Ocow 内部认证服务，配置 JWT 认证方案和授权策略。
builder.Services.AddOcowJwtAuthorization(builder.Configuration);

var app = builder.Build();

// 使用 Ocow OpenAPI 中间件，提供 Swagger 接口文档和测试界面。
app.UseOcowOpenApi();
// 使用 Ocow 请求链路追踪中间件，自动把 TraceId 注入到下游服务请求头中。
app.UseOcowRequestTrace();
// 使用 Ocow 统一异常处理中间件，自动把未处理异常转换成标准接口响应。
app.UseOcowExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Order.Api", status = "ok" }))
    .WithGroupName(OpenApiGroupNames.Health)
    .WithSummary("订单服务健康检查");

app.Run();

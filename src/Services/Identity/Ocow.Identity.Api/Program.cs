using Ocow.Identity.Application.Extensions;
using Ocow.Identity.Infrastructure.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 注册 Ocow 统一接口响应服务，自动把接口结果包装成标准格式。
builder.Services.AddOcowApiResponse();

// 注册 Ocow OpenAPI 服务，自动生成接口文档并提供测试界面。
builder.Services.AddOcowOpenApi(builder.Configuration);

// 注册身份认证应用服务。
builder.Services.AddIdentityApplication(builder.Configuration);

// 注册身份认证基础设施服务，配置数据库上下文和 Identity 框架。
builder.Services.AddIdentityInfrastructure(builder.Configuration);

// 注册 Ocow 内部认证服务，配置 JWT 认证方案和授权策略。
builder.Services.AddOcowJwtAuthorization(builder.Configuration);

var app = builder.Build();

// 使用 Ocow OpenAPI 中间件，提供swagger接口文档和测试界面。
app.UseOcowOpenApi();
// 使用 Ocow 请求链路追踪中间件，自动把 TraceId 注入到下游服务请求头中。
app.UseOcowRequestTrace();
// 使用 Ocow 统一异常处理中间件，自动把未处理的异常转换成标准接口响应。
app.UseOcowExceptionHandling();

app.UseAuthentication();

app.UseAuthorization(); 

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Identity.Api", status = "ok" }))
    .WithSummary("身份服务健康检查");

app.Run();

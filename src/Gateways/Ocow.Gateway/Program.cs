using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocow.Redis.Extensions;
using Ocow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 加载 Ocelot 配置文件，支持热更新。
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// 注册 Redis 连接、缓存、分布式锁和限流服务。
builder.Services.AddOcowRedis(builder.Configuration);
// 注册 Ocelot 服务。
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// 使用 Ocelot 请求链路追踪中间件，自动把 TraceId 注入到下游服务请求头中。
app.UseOcowRequestTrace();

// 使用 Ocelot 中间件处理请求转发。
await app.UseOcelot();

await app.RunAsync();

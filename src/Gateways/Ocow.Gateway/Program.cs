using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocow.Auth.Extensions;
using Ocow.Cache.Extensions;
using Ocow.Gateway.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOcowCache(builder.Configuration);
builder.Services.AddOcowAuth(builder.Configuration);
builder.Services.AddOcowInternalAuth(builder.Configuration);
builder.Services.AddOcowGatewaySecurity(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();
app.UseAuthentication();
app.UseOcowGatewaySecurity();
app.UseAuthorization();

await app.UseOcelot();

await app.RunAsync();

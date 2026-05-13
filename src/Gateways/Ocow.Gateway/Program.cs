using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();

await app.UseOcelot();

await app.RunAsync();

using Ocow.AspNetCore.Extensions;
using Ocow.Auth.Extensions;
using Ocow.BackgroundJobs.Extensions;
using Ocow.Cache.Extensions;
using Ocow.HealthChecks.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);
builder.Services.AddOcowHealthChecks(builder.Configuration, "Ocow.Jobs", checks =>
{
    checks.AddPostgreSqlCheck(builder.Configuration);
    checks.AddRedisCheck(builder.Configuration);
});
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOcowCache(builder.Configuration);
builder.Services.AddOcowAuth(builder.Configuration);
builder.Services.AddOcowBackgroundJobs(builder.Configuration);

var app = builder.Build();

app.UseOcowSwagger();
app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();
app.UseOcowExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();
app.UseOcowHangfireDashboard();

app.MapControllers();
app.MapOcowHealthChecks();

app.Run();

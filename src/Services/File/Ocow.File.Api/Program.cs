using Ocow.AspNetCore.Extensions;
using Ocow.Auth.Extensions;
using Ocow.Cache.Extensions;
using Ocow.Files.Application.Extensions;
using Ocow.Files.Infrastructure.Extensions;
using Ocow.HealthChecks.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);
builder.Services.AddOcowHealthChecks(builder.Configuration, "Ocow.File.Api", checks =>
{
    checks.AddPostgreSqlCheck(builder.Configuration);
});
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOcowCache(builder.Configuration);
builder.Services.AddFileApplication();
builder.Services.AddFileInfrastructure(builder.Configuration);
builder.Services.AddOcowAuth(builder.Configuration);
builder.Services.AddOcowInternalAuth(builder.Configuration);

var app = builder.Build();

app.UseOcowSwagger();
app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();
app.UseOcowExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapOcowHealthChecks();

app.Run();

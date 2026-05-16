using Ocow.Auth.Extensions;
using Ocow.AspNetCore.Extensions;
using Ocow.HealthChecks.Extensions;
using Ocow.Identity.Application.Extensions;
using Ocow.Identity.Infrastructure.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);

builder.Services.AddOcowHealthChecks(builder.Configuration, "Ocow.Identity.Api", checks =>
{
    checks.AddPostgreSqlCheck(builder.Configuration);
    checks.AddRedisCheck(builder.Configuration);
});

builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddIdentityApplication(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddOcowAuth(builder.Configuration);
builder.Services.AddOcowInternalAuth(builder.Configuration);  //引用这个有用吗

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

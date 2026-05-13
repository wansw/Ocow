using Ocow.Identity.Application.Extensions;
using Ocow.Identity.Infrastructure.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;
using Ocow.Shared.Extensions;
using Ocow.Shared.SwaggerApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddIdentityApplication(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddOcowJwtAuthorization(builder.Configuration);

var app = builder.Build();

app.UseOcowSwagger();
app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();
app.UseOcowExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Identity.Api", status = "ok" }))
    .WithGroupName(SwaggerApiGroupNames.Health)
    .WithSummary("身份服务健康检查");

app.Run();

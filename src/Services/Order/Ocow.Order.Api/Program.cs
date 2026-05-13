using Ocow.InternalAuth.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Order.Application.Extensions;
using Ocow.Order.Infrastructure.Extensions;
using Ocow.Redis.Extensions;
using Ocow.Shared.Extensions;
using Ocow.Shared.SwaggerApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOrderApplication();
builder.Services.AddOrderInfrastructure(builder.Configuration);
builder.Services.AddOcowJwtAuthorization(builder.Configuration);

var app = builder.Build();

app.UseOcowSwagger();
app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();
app.UseOcowExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Order.Api", status = "ok" }))
    .WithGroupName(SwaggerApiGroupNames.Health)
    .WithSummary("订单服务健康检查");

app.Run();

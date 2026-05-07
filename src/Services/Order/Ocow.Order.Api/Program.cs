using Ocow.InternalAuth.Extensions;
using Ocow.Order.Application.Extensions;
using Ocow.Order.Infrastructure.Extensions;
using Ocow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOcowApiResponse();
builder.Services.AddOcowOpenApi(builder.Configuration);
builder.Services.AddOrderApplication();
builder.Services.AddOrderInfrastructure(builder.Configuration);
builder.Services.AddOcowJwtAuthorization(builder.Configuration);

var app = builder.Build();

app.UseOcowOpenApi();
app.UseOcowRequestTrace();
app.UseOcowExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Order.Api", status = "ok" }))
    .WithSummary("订单服务健康检查");

app.Run();

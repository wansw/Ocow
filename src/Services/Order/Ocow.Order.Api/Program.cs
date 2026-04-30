using Ocow.Order.Application.Extensions;
using Ocow.Order.Infrastructure.Extensions;
using Ocow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOrderApplication();
builder.Services.AddOrderInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOcowRequestTrace();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Order.Api", status = "ok" }))
    .WithSummary("订单服务健康检查");

app.Run();

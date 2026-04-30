using Ocow.Identity.Application.Extensions;
using Ocow.Identity.Infrastructure.Extensions;
using Ocow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentityApplication(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOcowRequestTrace();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "Ocow.Identity.Api", status = "ok" }))
    .WithSummary("身份认证服务健康检查");

app.Run();

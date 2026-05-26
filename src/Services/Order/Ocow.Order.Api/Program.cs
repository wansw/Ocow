using Ocow.Auth.Extensions;
using Ocow.AspNetCore.Extensions;
using Ocow.Cache.Extensions;
using Ocow.Contracts.Events.Inventory;
using Ocow.Contracts.Events.Orders;
using Ocow.HealthChecks.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Observability.Extensions;
using Ocow.Order.Api.EventSubscribers;
using Ocow.Order.Application.Extensions;
using Ocow.Order.Infrastructure.EventHandlers;
using Ocow.Order.Infrastructure.Extensions;
using Ocow.Redis.Extensions;
using Ocow.EventBus.RabbitMq.Extensions;
using Ocow.ERP.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);
builder.Services.AddOcowHealthChecks(builder.Configuration, "Ocow.Order.Api", checks =>
{
    checks.AddPostgreSqlCheck(builder.Configuration);
    checks.AddRedisCheck(builder.Configuration);
});
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOcowCache(builder.Configuration);
builder.Services.AddOrderApplication();
builder.Services.AddOcowErp();
builder.Services.AddOrderInfrastructure(builder.Configuration);
builder.Services.AddOcowAuth(builder.Configuration);
builder.Services.AddOcowInternalAuth(builder.Configuration);
builder.Services.AddOcowCapRabbitMqEventBus(builder.Configuration);
builder.Services.AddIntegrationEventHandler<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
builder.Services.AddIntegrationEventHandler<OrderPaymentTimeoutIntegrationEvent, OrderPaymentTimeoutIntegrationEventHandler>();
builder.Services.AddIntegrationEventHandler<InventoryLockFailedIntegrationEvent, InventoryLockFailedIntegrationEventHandler>();
builder.Services.AddTransient<OrderCreatedIntegrationEventSubscriber>();
builder.Services.AddTransient<OrderPaymentTimeoutIntegrationEventSubscriber>();
builder.Services.AddTransient<InventoryLockFailedIntegrationEventSubscriber>();

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

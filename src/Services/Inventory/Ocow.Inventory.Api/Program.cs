using Ocow.Contracts.Events.Orders;
using Ocow.EventBus.RabbitMq.Extensions;
using Ocow.HealthChecks.Extensions;
using Ocow.Inventory.Api.EventSubscribers;
using Ocow.Inventory.Infrastructure.EventHandlers;
using Ocow.Inventory.Infrastructure.Extensions;
using Ocow.Observability.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowHealthChecks(builder.Configuration, "Ocow.Inventory.Api", checks =>
{
    checks.AddPostgreSqlCheck(builder.Configuration);
});
builder.Services.AddInventoryInfrastructure(builder.Configuration);
builder.Services.AddOcowCapRabbitMqEventBus(builder.Configuration);
builder.Services.AddIntegrationEventHandler<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
builder.Services.AddIntegrationEventHandler<OrderCanceledIntegrationEvent, OrderCanceledIntegrationEventHandler>();
builder.Services.AddIntegrationEventHandler<OrderPaidIntegrationEvent, OrderPaidIntegrationEventHandler>();
builder.Services.AddTransient<OrderCreatedIntegrationEventSubscriber>();
builder.Services.AddTransient<OrderCanceledIntegrationEventSubscriber>();
builder.Services.AddTransient<OrderPaidIntegrationEventSubscriber>();

var app = builder.Build();

app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();

app.MapControllers();
app.MapOcowHealthChecks();

app.Run();

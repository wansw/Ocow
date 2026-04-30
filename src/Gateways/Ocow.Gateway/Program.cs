using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseOcowRequestTrace();
await app.UseOcelot();
await app.RunAsync();

using Ocow.AspNetCore.Extensions;
using Ocow.Auth.Extensions;
using Ocow.BackgroundJobs.Extensions;
using Ocow.Cache.Extensions;
using Ocow.EntityFrameworkCore.Extensions;
using Ocow.HealthChecks.Extensions;
using Ocow.InternalAuth.Extensions;
using Ocow.Jobs.Api.Data;
using Ocow.Jobs.Api.Interfaces;
using Ocow.Jobs.Api.Jobs;
using Ocow.Jobs.Api.Options;
using Ocow.Jobs.Api.Services;
using Ocow.Observability.Extensions;
using Ocow.Redis.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddOcowObservability();

builder.Services.AddControllers();
builder.Services.AddOcowValidDtoResponse();
builder.Services.AddOcowSwagger(builder.Configuration);
builder.Services.AddOcowHealthChecks(builder.Configuration, "Ocow.Jobs", checks =>
{
    checks.AddPostgreSqlCheck(builder.Configuration);
    checks.AddRedisCheck(builder.Configuration);
});
builder.Services.AddOcowRedis(builder.Configuration);
builder.Services.AddOcowCache(builder.Configuration);
builder.Services.AddOcowAuth(builder.Configuration);
builder.Services.AddOcowInternalAuth(builder.Configuration);
builder.Services.AddOcowBackgroundJobs(builder.Configuration);
builder.Services.AddOcowDbContext<JobsDbContext>(builder.Configuration);
builder.Services.Configure<ServiceEndpointOption>(builder.Configuration.GetSection(ServiceEndpointOption.SectionName));
builder.Services.AddHttpClient<GenericHttpJob>().AddOcowInternalServiceAuthentication();
builder.Services.AddScoped<IJobScheduler, HangfireJobScheduler>();
builder.Services.AddScoped<IJobDefinitionService, JobDefinitionService>();

var app = builder.Build();

app.UseOcowSwagger();
app.UseOcowRequestTrace();
app.UseOcowSerilogRequestLogging();
app.UseOcowExceptionHandling();

app.UseAuthentication();
app.UseAuthorization();
app.UseOcowHangfireDashboard();

app.MapControllers();
app.MapOcowHealthChecks();

app.Run();

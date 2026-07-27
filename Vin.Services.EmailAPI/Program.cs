using Microsoft.EntityFrameworkCore;
using Serilog;
using Vin.BuildingBlocks.Diagnostics;
using Vin.BuildingBlocks.Health;
using Vin.MessageBus;
using Vin.Services.EmailAPI.Data;
using Vin.Services.EmailAPI.Messaging;
using Vin.Services.EmailAPI.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

var databaseConnection = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(databaseConnection))
{
    builder.Services.AddDbContextFactory<AppDbContext>(options =>
        options.UseSqlServer(databaseConnection));
}

builder.Services.AddSingleton<IEmailService, EmailService>();

builder.Services.AddMessageBusService(configBuilder =>
{
    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    if (builder.Environment.IsDevelopment())
    {
        configBuilder.AddUserSecrets<Program>();
    }
    configBuilder.AddEnvironmentVariables();
});

var serviceBusConfigured = !string.IsNullOrWhiteSpace(
    builder.Configuration["ServiceBus:ConnectionString"]);
var emailConsumerCanStart = serviceBusConfigured && !string.IsNullOrWhiteSpace(databaseConnection);

if (emailConsumerCanStart)
{
    builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();
    builder.Services.AddHostedService<AzureServiceBusConsumerHostedService>();
}

builder.Services.AddControllers();
builder.Services.AddVinServiceDefaults();
var healthChecks = builder.Services.AddHealthChecks();
if (!string.IsNullOrWhiteSpace(databaseConnection))
{
    healthChecks.AddDbContextReadinessCheck<AppDbContext>();
}
else
{
    healthChecks.AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Connection string is not configured."), tags: ["ready"]);
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseVinServiceDefaults();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapVinHealthChecks();

if (!serviceBusConfigured)
{
    app.Logger.LogWarning(
        "Email consumer is disabled because ServiceBus:ConnectionString is not configured.");
}
else if (string.IsNullOrWhiteSpace(databaseConnection))
{
    app.Logger.LogWarning(
        "Email consumer is disabled because ConnectionStrings:DefaultConnection is not configured.");
}

if (!string.IsNullOrWhiteSpace(databaseConnection))
{
    await ApplyMigrationAsync();
}

await app.RunAsync();

async Task ApplyMigrationAsync()
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await dbFactory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

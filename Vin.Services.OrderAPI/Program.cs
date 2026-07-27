using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vin.BuildingBlocks.Diagnostics;
using Vin.BuildingBlocks.Health;
using Vin.Eventing;
using Vin.Eventing.Abstractions;
using Vin.MessageBus;
using Vin.Services.OrderAPI.Data;
using Vin.Services.OrderAPI.Messaging;
using Vin.Services.OrderAPI.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
var apiSettings = builder.Configuration.GetSection("ApiSettings");
var secret = apiSettings["Secret"] ?? throw new InvalidOperationException("ApiSettings:Secret is not configured.");
var issuer = apiSettings["Issuer"] ?? throw new InvalidOperationException("ApiSettings:Issuer is not configured.");
var audience = apiSettings["Audience"] ?? throw new InvalidOperationException("ApiSettings:Audience is not configured.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddVinServiceDefaults();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextReadinessCheck<AppDbContext>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});
builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductAPI"]
        ?? throw new InvalidOperationException("ServiceUrls:ProductAPI is not configured.")))
    .AddVinResilience();
builder.Services.AddScoped<OrderApplicationService>();
builder.Services.AddScoped<IIntegrationEventHandler, OrderSagaEventHandler>();
builder.Services.AddOutboxPublisher<AppDbContext>();
builder.Services.AddIntegrationEventConsumer<AppDbContext>();
builder.Services.AddMessageBusService(config =>
{
    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    config.AddEnvironmentVariables();
});

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

await ApplyMigrationAsync();
await app.RunAsync();

async Task ApplyMigrationAsync()
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

using Microsoft.EntityFrameworkCore;
using Serilog;
using Vin.MessageBus;
using Vin.Services.EmailAPI.Data;
using Vin.Services.EmailAPI.Extension;
using Vin.Services.EmailAPI.Messaging;
using Vin.Services.EmailAPI.Services;


var builder = WebApplication.CreateBuilder(args);

// Add user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Configure Serilog
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration);
});

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var OptionsBuiler = new DbContextOptionsBuilder<AppDbContext>();
builder.Services.AddSingleton(new EmailService(OptionsBuiler.Options));

builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();

//builder.Services.AddSingleton<MessageBus>(sp =>
//{
//    var configuration = sp.GetRequiredService<IConfiguration>();
//    var logger = sp.GetRequiredService<ILogger<MessageBus>>();
//    return new MessageBus(configuration, logger); // Inject configuration and logger
//});
/*builder.Services.AddMessageBusService(builder.configure);*/

// Register MessageBus library and inject configuration
builder.Services.AddMessageBusService(configBuilder =>
{
    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    configBuilder.AddUserSecrets<Program>();
    configBuilder.AddEnvironmentVariables();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

ApplyMigration();
app.UseAzureServiceBusConsumer();

app.Run();


void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}
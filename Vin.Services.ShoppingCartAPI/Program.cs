using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Vin.MessageBus;
using Vin.Services.ShoppingCartAPI;
using Vin.Services.ShoppingCartAPI.Data;
using Vin.Services.ShoppingCartAPI.Service;
using Vin.Services.ShoppingCartAPI.Service.IService;
using Vin.Services.ShoppingCartAPI.Utility.Vin.Services.ShoppingCartAPI.Utility;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlServer(connectionString));

//Add auto mapper
IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddHttpClient("GetProduct",
    u => u.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductAPI"] ?? throw new InvalidOperationException("ServiceUrls:ProductAPI is not configured."))).AddHttpMessageHandler<AuthenticatedHttpClientHandler>();

builder.Services.AddHttpClient("GetCoupon",
    u => u.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CouponAPI"] ?? throw new InvalidOperationException("ServiceUrls:CouponAPI is not configured."))).AddHttpMessageHandler<AuthenticatedHttpClientHandler>();


builder.Services.AddScoped<AuthenticatedHttpClientHandler>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();


// Register IMessageBus


builder.Services.AddMessageBusService(configBuilder =>
{
    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    if (builder.Environment.IsDevelopment())
    {
        configBuilder.AddUserSecrets<Program>();
    }
    configBuilder.AddEnvironmentVariables();
});


builder.Services.AddHttpContextAccessor();



builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: 'Bearer Generated-JWT-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            }
        }, new string[]{ }
        }
    });
});

// Add this line at the beginning of the file to configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();


// Add this line to use Serilog as the logging provider
builder.Host.UseSerilog();

//Incase it cant regconize the apisettings, and usually it dont.
var settingSection = builder.Configuration.GetSection("ApiSettings");
var secret = settingSection["Secret"]
    ?? throw new InvalidOperationException("ApiSettings:Secret is not configured.");
var issuer = settingSection["Issuer"]
    ?? throw new InvalidOperationException("ApiSettings:Issuer is not configured.");
var audience = settingSection["Audience"]
    ?? throw new InvalidOperationException("ApiSettings:Audience is not configured.");
var key = Encoding.UTF8.GetBytes(secret);

builder.Services.AddAuthentication(x =>
{
    //Configures the authentication services for the application.

    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //DAS DefaultAuthenticateScheme: Specifies the default authentication method. Uses JWT bearer tokens, meaning that the application expects to receive JWTs for authentication.

    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;//DefaultChallengeScheme: This indicates how the application should respond when authentication fails. Here, it also uses JWT bearer tokens.
}).AddJwtBearer(x =>
{
    //Configures the JWT bearer authentication options.
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key), //the secret key just encoded before
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        ValidateAudience = true
    };
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    var endpoints = app.Services.GetRequiredService<IEnumerable<EndpointDataSource>>();
    foreach (var endpoint in endpoints.SelectMany(es => es.Endpoints))
    {

        if (!string.IsNullOrWhiteSpace(endpoint.DisplayName))
        {
            Log.Information("Mapped endpoint {Endpoint}", endpoint.DisplayName);
        }
    }

}


app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

ApplyMigration();

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

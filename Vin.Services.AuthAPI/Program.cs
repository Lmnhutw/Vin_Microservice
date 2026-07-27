using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vin.BuildingBlocks.Diagnostics;
using Vin.BuildingBlocks.Health;
using Vin.Services.AuthAPI.Data;
using Vin.Services.AuthAPI.Models;
using Vin.Services.AuthAPI.Services;
using Vin.Services.AuthAPI.Services.IServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Connection to SQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(connectionString);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders(); //register Identity,  ApplicationUser

var jwtSection = builder.Configuration.GetSection("ApiSettings:JwtOptions");
var jwtSecret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("ApiSettings:JwtOptions:Secret is not configured.");
var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("ApiSettings:JwtOptions:Issuer is not configured.");
var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("ApiSettings:JwtOptions:Audience is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>(); //register authService

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("ApiSettings:JwtOptions"))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Secret) &&
        !string.IsNullOrWhiteSpace(options.Issuer) &&
        !string.IsNullOrWhiteSpace(options.Audience),
        "ApiSettings:JwtOptions must define Secret, Issuer and Audience.")
    .ValidateOnStart();

builder.Services.AddControllers();
builder.Services.AddVinServiceDefaults();
builder.Services.AddHealthChecks().AddDbContextReadinessCheck<AppDbContext>();

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
app.UseVinServiceDefaults();

app.UseAuthentication();// MUST be put before authorize
app.UseAuthorization();

app.MapControllers();
app.MapVinHealthChecks();

ApplyMigration(); //applied migration method

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

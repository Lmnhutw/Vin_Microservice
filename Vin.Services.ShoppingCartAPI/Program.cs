using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Vin.Services.ShoppingCartAPI;
using Vin.Services.ShoppingCartAPI.Data;
using Vin.Services.ShoppingCartAPI.Service;
using Vin.Services.ShoppingCartAPI.Service.IService;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient("GetProduct",
    u => u.BaseAddress = new Uri(builder.Configuration["ServiceUrls:ProductAPI"]));

builder.Services.AddHttpClient("GetCoupon",
    u => u.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CouponAPI"]));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
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

//Incase it cant regconize the apisettings, and usually it dont.
var settingSection = builder.Configuration.GetSection("ApiSettings");

//Configure setting
var secret = settingSection.GetValue<string>("Secret");
var issurer = settingSection.GetValue<string>("Issuer");
var audience = settingSection.GetValue<string>("Audience");

//Create key and encoding base on 'secret'
var key = Encoding.ASCII.GetBytes(secret);

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
        ValidIssuer = issurer,
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
}
app.UseStaticFiles();

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
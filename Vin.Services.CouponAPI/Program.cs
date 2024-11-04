using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vin.Services.CouponAPI;
using Vin.Services.CouponAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

IMapper mapper = MappingConfig.InstanceMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//Configure setting
var secret = builder.Configuration.GetValue<string>("ApiSetting:Secret");
var issurer = builder.Configuration.GetValue<string>("ApiSetting:Issuer");
var audience = builder.Configuration.GetValue<string>("ApiSetting:Audience");

//Create key and encoding base on 'secrett;
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
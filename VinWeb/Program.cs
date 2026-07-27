using Microsoft.AspNetCore.Authentication.Cookies;
using NToastNotify;
using Vin.BuildingBlocks.Diagnostics;
using Vin.BuildingBlocks.Health;
using Vin.Web.Service;
using Vin.Web.Service.IService;
using Vin.Web.Utility;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddVinServiceDefaults();
builder.Services.AddHttpClient("VinAPI").AddVinResilience();

//Configure API
StaticDetail.CouponAPIBase = builder.Configuration["ServiceUrls:CouponAPI"];
StaticDetail.AuthAPIBase = builder.Configuration["ServiceUrls:AuthAPI"];
StaticDetail.ProductAPIBase = builder.Configuration["ServiceUrls:ProductAPI"];
StaticDetail.ShoppingCartAPIBase = builder.Configuration["ServiceUrls:ShoppingCartAPI"];
StaticDetail.OrderAPIBase = builder.Configuration["ServiceUrls:OrderAPI"];

// Toaster services
builder.Services.AddControllersWithViews()
    .AddNToastNotifyToastr(new ToastrOptions
    {
        ProgressBar = true,
        PositionClass = ToastPositions.BottomCenter,
        TimeOut = 5000
    });
builder.Services.AddHealthChecks().AddCheck("bff", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["ready"]);

builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IShoppingCartService, ShoppingCartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

//Add Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.ExpireTimeSpan = TimeSpan.FromDays(14);
        option.SlidingExpiration = true;
        option.Cookie.HttpOnly = true;
        option.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        option.Cookie.SameSite = SameSiteMode.Lax;
        option.LoginPath = "/Auth/login";
        option.AccessDeniedPath = "/Auth/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseNToastNotify();

app.UseHttpsRedirection();
app.UseVinServiceDefaults();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (builder.Configuration.GetValue("Frontend:UseAngular", true))
{
    app.MapFallbackToFile("index.html");
}
else
{
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}

app.MapVinHealthChecks();

app.Run();

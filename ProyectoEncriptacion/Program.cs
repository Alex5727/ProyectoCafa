using Data;
using Data.Interfaces;
using Data.Services;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Data.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IAesService>(_ =>
    new AesService("12345678901234567890123456789012")
);


builder.Services.AddMemoryCache();


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsersService, UsersService>();

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
var postgresConfig = new PostgresSQLConnection(connectionString);
builder.Services.AddSingleton(postgresConfig);

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("loginPolicy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
});


var PostgreSQLConnectionConfiguration = new PostgresSQLConnection(
    Environment.GetEnvironmentVariable("CONNECTION_STRING")
);
builder.Services.AddSingleton(PostgreSQLConnectionConfiguration);

// Autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build(); 

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.UseMiddleware<RateLimitMiddleware>();


app.UseAuthentication();
app.UseAuthorization();

// Rutas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

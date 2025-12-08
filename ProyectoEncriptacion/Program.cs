using Data;
using Data.Interfaces;
using Data.Services;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Data.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IAesService>(_ =>
{
    // MISMA clave que usabas antes y funcionaba
    return new AesService("12345678901234567890123456789012");
});

builder.Services.AddScoped<UserService>(sp =>
    new UserService(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddMemoryCache();


builder.Services.AddControllersWithViews();


string? connString = builder.Configuration.GetConnectionString("PostgresConnection");

var postgresConfig = new PostgresSQLConnection(connString);
builder.Services.AddSingleton(postgresConfig);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.UseDeveloperExceptionPage(); //aaaaa


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

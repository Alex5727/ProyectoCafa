using Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Data.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IAesService>(_ =>
    new AesService("12345678901234567890123456789012")
);


builder.Services.AddMemoryCache();


builder.Services.AddControllersWithViews();


var PostgreSQLConnectionConfiguration = new PostgresSQLConnection(
    Environment.GetEnvironmentVariable("CONNECTION_STRING")
);
builder.Services.AddSingleton(PostgreSQLConnectionConfiguration);


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth";
        options.AccessDeniedPath = "/Auth";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();


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


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


app.Run();

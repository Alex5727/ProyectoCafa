using Data;
using Data.Interfaces;
using Data.Services;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Data.Services;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);



// AES
builder.Services.AddSingleton<IAesService>(_ =>
{
    string aesKey = builder.Configuration["AesKey"]
        ?? throw new Exception("AesKey no existe");

    return new AesService(aesKey);
});


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddMemoryCache();


// MVC
builder.Services.AddControllersWithViews();


string? connString = builder.Configuration.GetConnectionString("PostgresConnection")
    ?? Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");

var postgresConfig = new PostgresSQLConnection(connString);
builder.Services.AddSingleton(postgresConfig);


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = false;

        // Seguridad de cookies
        options.Cookie.Name = ".NeonEncryption.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        // Validacion extra opcional
        options.Events.OnValidatePrincipal = async context =>
        {
            // Aquí puedes comprobar si el usuario sigue válido en BD
            // context.RejectPrincipal(); si quieres cerrar sesión
        };
    });


builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "dpkeys")))
    .SetApplicationName("NeonEncryption");

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("LoginPolicy", limiter =>
    {
        limiter.PermitLimit = 5;        // 5 intentos
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";

    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' https://cdn.jsdelivr.net; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net;";

    await next();
});


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
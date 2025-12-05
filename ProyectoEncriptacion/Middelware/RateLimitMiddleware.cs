using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.HttpOverrides;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    private const int LIMIT = 10;             // máximo POSTs permitidos en la ventana
    private const int WINDOW_SECONDS = 60;    // ventana para contar POSTs
    private const int BLOCK_MINUTES = 10;     // tiempo de bloqueo cuando se supera el límite

    public RateLimitMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = GetClientIp(context) ?? "unknown";

        var blockKey = $"Blocked_{ip}";
        if (_cache.TryGetValue(blockKey, out DateTime blockedUntil) && blockedUntil > DateTime.UtcNow)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "text/html; charset=utf-8";
            var retrySeconds = (int)(blockedUntil - DateTime.UtcNow).TotalSeconds;
            await context.Response.WriteAsync($@"
                    <html>
                    <head>
                        <title>Demasiadas Peticiones</title>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                                text-align: center;
                                background: #f4f4f4;
                                padding-top: 60px;
                            }}
                            .card {{
                                background: white;
                                max-width: 400px;
                                margin: auto;
                                padding: 20px;
                                border-radius: 12px;
                                box-shadow: 0px 3px 10px rgba(0,0,0,0.2);
                            }}
                            .title {{
                                font-size: 24px;
                                font-weight: bold;
                                color: #c0392b;
                            }}
                            .subtitle {{
                                margin-top: 10px;
                                font-size: 16px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='card'>
                            <div class='title'>Demasiadas solicitudes</div>
                            <div class='subtitle'>
                                Has superado el límite permitido de <strong>{LIMIT}</strong> peticiones.<br><br>
                                Por favor intenta de nuevo en <strong>{retrySeconds}s</strong>.
                            </div>
                        </div>
                    </body>
                    </html>
                    ");
                    return;

        }

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // 4) Contador por IP en MemoryCache
        var countKey = $"RateLimit_{ip}";
        var entry = _cache.GetOrCreate(countKey, entryCache =>
        {
            entryCache.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(WINDOW_SECONDS);
            return new RateLimitEntry { Count = 0, ExpiresAt = DateTime.UtcNow.AddSeconds(WINDOW_SECONDS) };
        });

        entry.Count++;

        _cache.Set(countKey, entry, entry.ExpiresAt);

        if (entry.Count > LIMIT)
        {
          
            var blockUntil = DateTime.UtcNow.AddMinutes(BLOCK_MINUTES);
            _cache.Set(blockKey, blockUntil, blockUntil); 

            _cache.Remove(countKey);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"Has excedido el límite de {LIMIT} POSTs. Estás bloqueado por {BLOCK_MINUTES} minutos.");
            return;
        }

  
        await _next(context);
    }

    private string? GetClientIp(HttpContext context)
    {
        var xff = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xff))
        {
            var first = xff.Split(',').Select(s => s.Trim()).FirstOrDefault();
            if (IPAddress.TryParse(first, out var parsed1))
            {
                return NormalizeIp(parsed1);
            }
        }

        // Revisar X-Real-IP
        var xr = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xr) && IPAddress.TryParse(xr, out var parsed2))
        {
            return NormalizeIp(parsed2);
        }

        var remote = context.Connection.RemoteIpAddress;
        if (remote != null)
            return NormalizeIp(remote);

        return null;
    }

    // Normalizar IPv6 mapped IPv4 a IPv4, y devolver string
    private string NormalizeIp(IPAddress ip)
    {
        if (ip.IsIPv4MappedToIPv6)
        {
            return ip.MapToIPv4().ToString();
        }
        return ip.ToString();
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
using System.Security.Claims;
using Data.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Models;
using Data.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProyectoEncriptacion.Controllers
{
    [Route("Auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("Login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al home
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Login(LoginDTO loginRequestData)
        {
            try
            {
                // Validación básica
                if (string.IsNullOrEmpty(loginRequestData.Username) ||
                    string.IsNullOrEmpty(loginRequestData.Password))
                {
                    TempData["Error"] = "Usuario y contraseña son requeridos";
                    return RedirectToAction("Login");
                }

                var user = await _authService.Login(loginRequestData);

                // Crear sesión con claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.username)
                };

                var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                };
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Redirección
                return RedirectToAction("Index", "Home");
            }
            catch (HttpResponseException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                TempData["Error"] = "Error de autenticación. Verifique sus credenciales.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            return RedirectToAction("Login", "Auth");
        }
    }
}
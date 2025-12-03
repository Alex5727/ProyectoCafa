using System.Security.Claims;
using Data.Interfaces;
using DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Models;

namespace ProyectoEncriptacion.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [Route("auth")]
        // [Route("login")]
        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Auth()
        {
            return View();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO loginRequestData)
        {
            UsuarioModel user = await _authService.Login(loginRequestData);
            if (user == null)
                return Unauthorized("Credenciales incorrectas");

            // Claims del usuario
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.username),
        };

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            await HttpContext.SignInAsync(
                scheme: "Cookies",
                principal: new ClaimsPrincipal(claimsIdentity),
                properties: new AuthenticationProperties
                {
                    IsPersistent = true,        // cookie persistente
                    ExpiresUtc = DateTime.UtcNow.AddHours(8)
                }
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Auth");
        }

    }

}


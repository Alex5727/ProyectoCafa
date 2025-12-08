using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Npgsql;
using BCrypt.Net;
using Data.Interfaces; // IUsersService
using ProyectoEncriptacion.Models;
using Microsoft.AspNetCore.RateLimiting;
using Data;
using Microsoft.AspNetCore.Authorization;
using Data.Exceptions;
using System.Text.RegularExpressions; // RegisterViewModel

public class RegistroController : Controller
{
    private readonly IUsersService _usersService;

    public RegistroController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Sanitizar / normalizar username (opcional)
        var username = model.Username.Trim();

        // Validación extra del servidor (ya hay DataAnnotations, pero reforzamos)
        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]{3,50}$"))
        {
            ModelState.AddModelError(nameof(model.Username), "Usuario inválido");
            return View(model);
        }

        if (model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Las contraseñas no coinciden");
            return View(model);
        }

        try
        {
            // Llamada segura al servicio (que usa parámetros con Dapper)
            await _usersService.CreateUserAsync(username, model.Password);

            TempData["Success"] = "Usuario registrado correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login", "Auth");
        }
        catch (PostgresException ex)
        {
            // Manejo de la excepción que lanza tu función (p. ej. P0001 cuando usuario ya existe)
            if (ex.SqlState == "P0001") // el código que usaste en la función PL/pgSQL para usuario duplicado
            {
                TempData["Error"] = $"El usuario {username} ya existe.";
                return RedirectToAction("Register");
            }

            // Unique violation clásico (si la DB lanza el código estándar)
            if (ex.SqlState == PostgresErrorCodes.UniqueViolation) // ver nota
            {
                TempData["Error"] = $"El usuario {username} ya existe.";
                return RedirectToAction("Register");
            }

            // Para otras excepciones específicas de BD no mostrar el detalle al usuario
            TempData["Error"] = "Error al crear el usuario. Intenta nuevamente más tarde.";
            // aquí podrías loguear ex.Message en logs del servidor
            return RedirectToAction("Register");
        }
        catch (Exception)
        {
            TempData["Error"] = "Error inesperado. Intenta de nuevo más tarde.";
            return RedirectToAction("Register");
        }
    }
}
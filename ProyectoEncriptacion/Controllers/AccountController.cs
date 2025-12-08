using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Models;

namespace ProyectoEncriptacion.Controllers
{
    public class RegistroController : Controller
    {
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Aquí podrías guardar el usuario en una BD
            // Ejemplo:
            // _userService.Create(model.Username, model.Email, model.Password);

            ViewBag.Message = "Usuario registrado correctamente";
            return View("Register");
        }
    }
}

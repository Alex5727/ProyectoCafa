using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{
    public class AuthController : Controller
    {
        [Route("auth")]
        [Route("login")]
        public IActionResult Auth()
        {
            return View();
        }


    }
}

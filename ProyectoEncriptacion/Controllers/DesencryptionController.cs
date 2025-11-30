using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{

    public class DesencryptionController : Controller
    {
        [Route("desencryption")]
        [Route("decrypt")]
        public IActionResult Desencryption()
        {
            return View();
        }
    }
}

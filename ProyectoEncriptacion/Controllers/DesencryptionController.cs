using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{

    public class DesencryptionController : Controller
    {
        [Route("desencryption")]
        [Route("decrypt")]
        [Authorize]
        public IActionResult Desencryption()
        {
            return View();
        }
    }
}

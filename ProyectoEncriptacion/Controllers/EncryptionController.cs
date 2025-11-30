using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{
    public class EncryptionController : Controller
    {
        [Route("encryption")]
        [Route("ecrypt")]
        public IActionResult Encryption()
        {
            return View();
        }
    }
}

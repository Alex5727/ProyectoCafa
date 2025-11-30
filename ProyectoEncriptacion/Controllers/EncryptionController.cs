using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{
    public class EncryptionController : Controller
    {
        [Route("encryption")]
        [Route("ecrypt")]
        [Authorize]
        public IActionResult Encryption()
        {
            return View();
        }
    }
}

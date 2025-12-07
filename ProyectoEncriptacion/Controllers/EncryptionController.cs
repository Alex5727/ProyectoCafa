
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Models;

namespace ProyectoEncriptacion.Controllers
{
    [Authorize]
    [Route("encryption")]
    [Route("encrypt")]
    public class EncryptionController : Controller
    {
        private readonly IAesService _aesService;

        // PRIMERO el constructor SIEMPRE
        public EncryptionController(IAesService aesService)
        {
            _aesService = aesService;
        }

        // PANTALLA PRINCIPAL
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View("Encryption", new EncryptionViewModel());
        }

        // POST
        [HttpPost("index")]
        public IActionResult Index(EncryptionViewModel model)
        {
            if (!string.IsNullOrEmpty(model.InputText))
            {
                model.OutputText = _aesService.Encrypt(model.InputText);
            }

            return View("Encryption", model);
        }
    }

}
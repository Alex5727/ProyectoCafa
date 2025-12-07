
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Models;



namespace ProyectoEncriptacion.Controllers
{
    [Route("encryption")]
    [Route("ecrypt")]
    [Authorize]
    public class EncryptionController : Controller
    {
        private readonly IAesService _aesService;


        public IActionResult Encryption()
        {
            var model = new EncryptionViewModel();
            // inicializa propiedades necesarias, por ejemplo:

            return View(model);
        }
        public EncryptionController(IAesService aesService)
        {
            _aesService = aesService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Encryption", new EncryptionViewModel());
        }

        [HttpPost]
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
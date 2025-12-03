using Data.Interfaces;
using Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Models;


namespace ProyectoEncriptacion.Controllers
{

    public class DesencryptionController : Controller
    {
        private readonly IAesService _aes;

        public DesencryptionController(IAesService aes)
        {
            _aes = aes;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("desencryption", new TextModel());
        }

        [HttpPost]
        public IActionResult Index(TextModel model)
        {
            if (string.IsNullOrWhiteSpace(model.InputText))
            {
                ModelState.AddModelError("", "Ingresa un texto encriptado.");
                return View("desencryption", model);
            }

            try
            {
                model.DecryptedText = _aes.Decrypt(model.InputText);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al desencriptar: " + ex.Message);
            }

            return View("desencryption", model);
        }
    }
}
using System.Security.Cryptography;
using Data.Interfaces;
using Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoEncriptacion.Data.Interfaces;
using ProyectoEncriptacion.Models;


    namespace ProyectoEncriptacion.Controllers
    {
        [Route("desencryption")]
        [Authorize]
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
                return View("Desencryption", new TextModel());
            }

            [HttpPost]
            public IActionResult Index(TextModel model)
            {
                if (string.IsNullOrWhiteSpace(model.InputText))
                {
                    ModelState.AddModelError("", "Ingresa un texto encriptado.");
                    return View("Desencryption", model);
                }

                try
                {
                    model.DecryptedText = _aes.Decrypt(model.InputText);
                }
                catch (ArgumentException ex)
                {
                    // errores de formato / base64 / longitud
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
                catch (CryptographicException ex)
                {
                    // fallo de autenticación
                    ModelState.AddModelError("", "Error de autenticación al desencriptar: " + ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error inesperado al desencriptar: " + ex.Message);
                }

                return View("Desencryption", model);
            }
        }
    }

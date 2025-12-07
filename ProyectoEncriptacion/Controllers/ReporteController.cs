using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{
    [Route("report")]
    [Authorize]  // quitar si quieres que cualquiera pueda verla
    public class ReportController : Controller
    {
        // GET: /report o /report/index
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View("Report");
        }
    }
}
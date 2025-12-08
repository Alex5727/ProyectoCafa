using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ProyectoEncriptacion.Controllers
{
    [EnableRateLimiting("GeneralPolicy")]
    [Route("usersmanagement")]
    [Route("users")]
    [Authorize]
    public class UsersManagementController : Controller
    {
        [HttpGet("")]
        [HttpGet("index")]
        public IActionResult Index()
        {
            return View("UsersManagement");
        }
    }
}
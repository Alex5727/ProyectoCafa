using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{
    public class UsersManagementController : Controller
    {
        [Route("usersmanagement")]
        [Route("users")]
        [Authorize]
        public IActionResult UsersManagement()
        {
            return View();
        }
    }
}

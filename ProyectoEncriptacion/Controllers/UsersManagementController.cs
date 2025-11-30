using Microsoft.AspNetCore.Mvc;

namespace ProyectoEncriptacion.Controllers
{
    public class UsersManagementController : Controller
    {
        [Route("usersmanagement")]
        [Route("users")]
        public IActionResult UsersManagement()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoPortal.Controllers
{
    [Authorize("Admin")]
    public class AdminController : Controller
    {
        [HttpGet]
        public IActionResult factoryManagement()
        {
            return View();
        }
    }
}

using AutoPortal.Libs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace AutoPortal.Controllers
{
    [Authorize("Admin")]
    public class AdminController : BaseController
    {
        public AdminController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        {}

        [HttpGet]
        public IActionResult factoryManagement()
        {
            return View();
        }
    }
}

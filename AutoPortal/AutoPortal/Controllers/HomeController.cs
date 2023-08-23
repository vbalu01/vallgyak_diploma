using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Diagnostics;

namespace AutoPortal.Controllers
{
    public class HomeController : BaseController
    {
        /*private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }*/

        public HomeController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult MyProfile()
        {
            switch (loginType)
            {
                case eVehicleTargetTypes.DEALER:
                    ViewBag.loginType = "Kereskedő";
                    ViewBag.pageCategory = 2;
                    ViewBag.id = user.id;
                    ViewBag.email = user.email;
                    ViewBag.phone = user.phone;
                    ViewBag.name = user.name;
                    ViewBag.description = user.description;
                    ViewBag.statuses = EnumHelper.GetStatusStringList(user.status);
                    ViewBag.country = user.country;
                    ViewBag.city = user.city;
                    ViewBag.address = user.address;
                    ViewBag.website = user.website;
                    break;
                case eVehicleTargetTypes.SERVICE:
                    ViewBag.loginType = "Szerviz";
                    ViewBag.pageCategory = 1;
                    ViewBag.id = user.id;
                    ViewBag.email = user.email;
                    ViewBag.phone = user.phone;
                    ViewBag.name = user.name;
                    ViewBag.description = user.description;
                    ViewBag.statuses = EnumHelper.GetStatusStringList(user.status);
                    ViewBag.country = user.country;
                    ViewBag.city = user.city;
                    ViewBag.address = user.address;
                    ViewBag.website = user.website;
                    break;
                case eVehicleTargetTypes.USER:
                    ViewBag.loginType = "Felhasználó";
                    ViewBag.pageCategory = 0;
                    ViewBag.id = user.id;
                    ViewBag.email = user.email;
                    ViewBag.name = user.name;
                    ViewBag.register = user.register_date;
                    ViewBag.statuses = EnumHelper.GetStatusStringList(user.status);
                    ViewBag.roles = user.GetRoles();
                    break;
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
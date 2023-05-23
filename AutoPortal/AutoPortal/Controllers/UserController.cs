using AutoPortal.Libs;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace AutoPortal.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        public UserController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        {
        }

        public IActionResult addCar()
        {
            ViewBag.vehicleCategories = _SQL.vehicleCategories.ToList();
            ViewBag.makes = _SQL.vehicleMakes.ToList();
            ViewBag.models = _SQL.vehicleModels.ToList();
            ViewBag.fuels = _SQL.fuelTypes.ToList();
            ViewBag.transmissions = _SQL.transmissionTypes.ToList();
            ViewBag.drives = _SQL.driveTypes.ToList();
            ViewBag.bodyTypes = _SQL.bodyTypes.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> addNewUserCar([FromBody]AddUserCarModel m)
        {
            return Ok();
        }
    }
}

using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using AutoPortal.Models.ResponseModels;
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

        public IActionResult userManagement()
        {
            ViewBag.Users = _SQL.users.ToList();
            return View();
        }

        public IActionResult serviceManagement()
        {
            ViewBag.Services = _SQL.services.ToList();
            return View();
        }

        public IActionResult dealerManagement()
        {
            ViewBag.Dealers = _SQL.dealers.ToList();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> getUserData(int userId)
        {
            dynamic returnModel = new System.Dynamic.ExpandoObject(); ;
            User findUser = _SQL.users.SingleOrDefault(u => u.id == userId);

            if (findUser == null)
            {
                _Notification.AddErrorToastMessage("A keresett felhasználó nem található!");
                return BadRequest();
            }

            List<UserVehicle> vehicles = new List<UserVehicle>();
            foreach (var item in _SQL.vehiclePermissions.Where(i => i.target_type == eVehicleTargetTypes.USER && i.target_id == userId))
            {
                using (SQL mysql = new SQL())
                {
                    vehicles.Add(new UserVehicle { p = item.permission, v = mysql.vehicles.SingleOrDefault(vh => vh.chassis_number == item.vehicle_id) });
                }
            }


            findUser.password = null;

            returnModel.User = findUser;
            returnModel.UserVehicles = vehicles;

            return Ok(returnModel);
        }
    }
}

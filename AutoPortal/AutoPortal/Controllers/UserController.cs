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

        public IActionResult myCars()
        {
            List<UserVehicle> vehicles = new List<UserVehicle>();
            foreach (var item in _SQL.vehiclePermissions.Where(i => i.target_type == loginType && i.target_id == loginId))
            {
                using(SQL mysql = new SQL()) {
                    vehicles.Add(new UserVehicle { p = item.permission, v = mysql.vehicles.SingleOrDefault(vh => vh.chassis_number == item.vehicle_id) });
                }
            }

            if(vehicles.Count > 0)
                ViewBag.vehicles = vehicles;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> addNewUserCar([FromBody]AddUserCarModel m)
        {
            if(ModelState.IsValid) {
                if (_SQL.vehicles.Any(v => v.chassis_number == m.chassis_number)) { //Már rögzítve lett a jármű
                    _Notification.AddWarningToastMessage("A megadott alvázszám már szerepel a rendszerben!\nKérjük vegye fel a kapcsolatot velünk.");
                    return Conflict();
                }

                Vehicle v = new Vehicle(m);
                _SQL.vehicles.Add(v);
                await _SQL.SaveChangesAsync();

                _SQL.vehiclePermissions.Add(new VehiclePermission { vehicle_id = v.chassis_number, target_id = loginId, target_type = loginType, permission = eVehiclePermissions.OWNER});
                await _SQL.SaveChangesAsync();

                _Notification.AddSuccessToastMessage("Jármű sikeresen rögzítve!");
                return Ok();

            }
            else {
                _Notification.AddErrorToastMessage("Hibás adatok!");
                return BadRequest();
            }
            
        }
    }
}

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

#region Views
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
                using (SQL mysql = new SQL()) {
                    vehicles.Add(new UserVehicle { p = item.permission, v = mysql.vehicles.SingleOrDefault(vh => vh.chassis_number == item.vehicle_id) });
                }
            }

            if (vehicles.Count > 0)
                ViewBag.vehicles = vehicles;
            return View();
        }

        public IActionResult manageVehicle(string vehicleId)
        {
            Vehicle v = _SQL.vehicles.SingleOrDefault(ve => ve.chassis_number == vehicleId);

            if (v == null) //Nincs ilyen jármű
                return NotFound();

            VehiclePermission vp = _SQL.vehiclePermissions.SingleOrDefault(p => p.target_type == loginType && p.target_id == loginId);

            if (vp == null) //Nincs semmilyen jogosultsága a felhasználónak a járműhöz
                return Forbid();

            List<(DateTime, int)> mileageStands = new();

            foreach (MileageStand ms in MileageStand.GetVehicleMileageStands(vehicleId)) {
                mileageStands.Add((ms.date, ms.mileage));
            }
            foreach(ServiceEvent se in ServiceEvent.GetVehicleServiceEvents(vehicleId)) { 
                mileageStands.Add((se.date, se.mileage));
            }

            ViewBag.Refuels = Refuel.GetVehicleRefuels(vehicleId);
            ViewBag.OtherCosts = OtherCost.GetVehicleOtherCosts(vehicleId);
            ViewBag.MileageStands = mileageStands.OrderBy(tmp => tmp.Item1).ToList();

            ViewBag.vehicleData = new UserVehicle() { p = vp.permission, v = v };
            return View();
        }

        public IActionResult addServiceEvent()
        {
            if (((Service)user).isValid())
                return View();
            else
            {
                _Notification.AddErrorToastMessage("A funkció nem elérhető.\nA felhasználó nincs megerősítve, vagy blokkolva van.");
                return Redirect("/");
            }
        }

        #endregion
        

        #region handleRequests
        [HttpPost]
        public async Task<IActionResult> addNewUserCar([FromBody] AddUserCarModel m)
        {
            if (ModelState.IsValid) {
                if (_SQL.vehicles.Any(v => v.chassis_number == m.chassis_number)) { //Már rögzítve lett a jármű
                    _Notification.AddWarningToastMessage("A megadott alvázszám már szerepel a rendszerben!\nKérjük vegye fel a kapcsolatot velünk.");
                    return Conflict();
                }

                Vehicle v = new Vehicle(m);
                _SQL.vehicles.Add(v);
                await _SQL.SaveChangesAsync();

                _SQL.vehiclePermissions.Add(new VehiclePermission { vehicle_id = v.chassis_number, target_id = loginId, target_type = loginType, permission = eVehiclePermissions.OWNER });
                await _SQL.SaveChangesAsync();

                _Notification.AddSuccessToastMessage("Jármű sikeresen rögzítve!");
                return Ok();

            }
            else {
                _Notification.AddErrorToastMessage("Hibás adatok!");
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<IActionResult> addNewRefuel([FromBody]AddNewRefuelModel m)
        {
            if (ModelState.IsValid)
            {
                if(!_SQL.vehicles.Any(v=>v.chassis_number == m.vehicle_id)) {
                    _Notification.AddErrorToastMessage("Jármű nem található!");
                    return NotFound();
                }


                VehiclePermission vp = _SQL.vehiclePermissions.SingleOrDefault(v => v.target_type == loginType && v.target_id == loginId);

                if(vp == null || vp.permission == eVehiclePermissions.NONE) {
                    _Notification.AddErrorToastMessage("Nincs jogosultsága a járműhöz!");
                    return Forbid();
                }

                Refuel rf = new Refuel(m);
                await _SQL.refuels.AddAsync(rf);
                await _SQL.SaveChangesAsync();

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!");
                return Ok();
            }
            _Notification.AddErrorToastMessage("Hiányos adatok!");
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> addNewCost([FromBody]AddNewCostModel m)
        {
            if (ModelState.IsValid)
            {
                if (!_SQL.vehicles.Any(v => v.chassis_number == m.vehicle_id))
                {
                    _Notification.AddErrorToastMessage("Jármű nem található!");
                    return NotFound();
                }


                VehiclePermission vp = _SQL.vehiclePermissions.SingleOrDefault(v => v.target_type == loginType && v.target_id == loginId);

                if (vp == null || (!vp.permission.HasFlag(eVehiclePermissions.OWNER) && !vp.permission.HasFlag(eVehiclePermissions.SUBOWNER)))
                {
                    _Notification.AddErrorToastMessage("Nincs jogosultsága a járműhöz!");
                    return Forbid();
                }

                OtherCost oc = new OtherCost(m);
                await _SQL.otherCosts.AddAsync(oc);
                await _SQL.SaveChangesAsync();

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!");
                return Ok();
            }
            _Notification.AddErrorToastMessage("Hiányos adatok!");
            return BadRequest();
        }
        #endregion
    }
}

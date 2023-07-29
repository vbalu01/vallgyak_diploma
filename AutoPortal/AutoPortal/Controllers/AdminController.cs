using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using AutoPortal.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Security.Claims;

namespace AutoPortal.Controllers
{
    [Authorize("Admin")]
    public class AdminController : BaseController
    {
        public AdminController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        {}

            #region Views
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

        public IActionResult vehicleManagement()
        {
            ViewBag.Vehicles = _SQL.vehicles.ToList();
            return View();
        }

        public IActionResult vehicleHandler(string vehicleId)
        {
            Vehicle vehicle = _SQL.vehicles.SingleOrDefault(v=>v.chassis_number == vehicleId);
            ViewBag.Vehicle = vehicle;
            return View();
        }

        #endregion

        #region UserRegion
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

            List<UserRole> userRoles = _SQL.userRoles.Where(r=>r.userId == userId).ToList();
            List<Role> roles = _SQL.roles.ToList();


            findUser.password = null;

            returnModel.User = findUser;
            returnModel.UserVehicles = vehicles;

            returnModel.UserRoles = userRoles;
            returnModel.Roles = roles;

            return Ok(returnModel);
        }

        [HttpPost]
        public async Task<IActionResult> updateUserData([FromBody] AdminUpdateUserModel m)
        {
            if (ModelState.IsValid) {
                if(_SQL.users.Any(u=>u.id == m.id)) {
                    User u = _SQL.users.Single(usr=>usr.id == m.id);
                    if (u.email != m.email)
                        u.email = m.email;
                    if(u.name != m.userName)
                        u.name = m.userName;
                    if(u.status != m.status)
                        u.status = m.status;

                    _SQL.users.Update(u);

                    foreach(string role in m.roles)
                    {
                        if(_SQL.roles.Any(r=>r.role == role))
                        {
                            if(!_SQL.userRoles.Any(ur=>ur.userId == m.id && ur.roleId == role))
                            {
                                _SQL.userRoles.Add(new UserRole() { roleId = role, userId = m.id });
                            }
                        }
                    }

                    foreach(var urole in _SQL.userRoles.Where(ur=>ur.userId == m.id))
                    {
                        if (!m.roles.Any(r => r == urole.roleId))
                            _SQL.userRoles.Remove(urole);
                    }

                    
                    

                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Módosítások mentése sikeres!");
                    return Ok("Módosítások sikeresen elmentve!");
                }
                else {
                    _Notification.AddErrorToastMessage("Sikertelen mentés: Felhasználó nem található!");
                    return NotFound("A felhasználó nem található!");
                }
            }
            else {
                _Notification.AddErrorToastMessage("Módosítás sikertelen: Helytelen adatok!");
                return BadRequest("Hibás adatok!");
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> updateUserVehiclePerm(eVehiclePermissions perm, string vehId, eVehicleTargetTypes uType, int uid)
        {
            if(!_SQL.vehicles.Any(v=>v.chassis_number == vehId)) //Nem a létezik a jármű
            {
                _Notification.AddErrorToastMessage("A keresett jármű nem található!");
                return NotFound("A keresett jármű nem található!");
            }

            switch (uType) //Felhasználó létezik? => Jogosultság adás
            {
                case eVehicleTargetTypes.NONE:
                    _Notification.AddErrorToastMessage("Hibás célcsoport!");
                    return BadRequest("Hibás célcsoport!");
                    break;
                case eVehicleTargetTypes.USER:
                    if(!_SQL.users.Any(u=>u.id == uid))
                    {
                        _Notification.AddErrorToastMessage("A keresett felhasználó nem található!");
                        return NotFound("Felhasználó nem található!");
                    }
                    break;
                case eVehicleTargetTypes.SERVICE:
                    if (!_SQL.services.Any(u => u.id == uid))
                    {
                        _Notification.AddErrorToastMessage("A keresett szerviz nem található!");
                        return NotFound("Felhasználó nem található!");
                    }
                    break;
                case eVehicleTargetTypes.DEALER:
                    if (!_SQL.dealers.Any(u => u.id == uid))
                    {
                        _Notification.AddErrorToastMessage("A keresett kereskedő nem található!");
                        return NotFound("Felhasználó nem található!");
                    }
                    break;
                case eVehicleTargetTypes.FACTORY:
                    if (!_SQL.factories.Any(u => u.id == uid))
                    {
                        _Notification.AddErrorToastMessage("A keresett gyártó nem található!");
                        return NotFound("Felhasználó nem található!");
                    }
                    break;
            }

            if (_SQL.vehiclePermissions.Any(vp => vp.target_type == uType && vp.target_id == uid && vp.vehicle_id == vehId))
            {
                if (perm == eVehiclePermissions.NONE)
                {
                    VehiclePermission toRemove = _SQL.vehiclePermissions.Single(vp => vp.target_type == uType && vp.target_id == uid && vp.vehicle_id == vehId);
                    _SQL.vehiclePermissions.Remove(toRemove);
                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                    return Ok("Sikeres módosítás!");
                }
                else
                {
                    VehiclePermission toUpdate = _SQL.vehiclePermissions.Single(vp => vp.target_type == uType && vp.target_id == uid && vp.vehicle_id == vehId);
                    toUpdate.permission = perm;
                    _SQL.vehiclePermissions.Update(toUpdate);
                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                    return Ok("Sikeres módosítás");
                }
            }
            else
            {
                if (perm == eVehiclePermissions.NONE)
                {
                    _Notification.AddInfoToastMessage("Nem történt módosítás");
                    return Ok("Nem történt módosítás");
                }
                else
                {
                    _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = perm, target_id = uid, target_type = uType, vehicle_id = vehId });
                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                    return Ok("Sikeres módosítás");
                }
            }
        }
        #endregion


        }
}

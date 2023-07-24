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
        #endregion


    }
}

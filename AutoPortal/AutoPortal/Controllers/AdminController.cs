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
            Vehicle v = _SQL.vehicles.SingleOrDefault(ve => ve.chassis_number == vehicleId);

            if (v == null) //Nincs ilyen jármű
                return NotFound();

            List<MileageStandModel> mileageStands = v.getMileageStands();

            ViewBag.Refuels = Refuel.GetVehicleRefuels(vehicleId);
            ViewBag.OtherCosts = OtherCost.GetVehicleOtherCosts(vehicleId);
            ViewBag.MileageStands = mileageStands.OrderBy(tmp => tmp.RecordedDate).ToList();
            ViewBag.Vehicle = v;
            ViewBag.ServiceEvents = _SQL.serviceEvents.Where(tmp=>tmp.vehicle_id == vehicleId).ToList();

            List<Service> services = _SQL.services.ToList();
            foreach (Service service in services)
            {
                service.password = null;
            }

            ViewBag.Services = services;

            List<(string, eVehiclePermissions)> vehiclePermissions = new();

            List<VehiclePermission> tmpPerm = _SQL.vehiclePermissions.Where(tmp => tmp.vehicle_id == vehicleId).ToList();

            foreach (VehiclePermission vp in tmpPerm)
            {
                string mail = "";
                if(vp.target_type == eVehicleTargetTypes.DEALER)
                {
                    mail = _SQL.dealers.Single(tp => tp.id == vp.target_id).email;
                }
                else if (vp.target_type == eVehicleTargetTypes.SERVICE)
                {
                    mail = _SQL.services.Single(tp => tp.id == vp.target_id).email;
                }
                else if (vp.target_type == eVehicleTargetTypes.FACTORY)
                {
                    mail = _SQL.factories.Single(tp => tp.id == vp.target_id).email;
                }
                else if (vp.target_type == eVehicleTargetTypes.USER)
                {
                    mail = _SQL.users.Single(tp => tp.id == vp.target_id).email;
                }
                else if (vp.target_type == eVehicleTargetTypes.NONE)
                {
                    mail = "?";
                }
                vehiclePermissions.Add((mail, vp.permission));
            }

            ViewBag.VehiclePermissions = vehiclePermissions;

            return View();
        }

        #endregion

        #region UserRegion
        [HttpGet]
        public async Task<IActionResult> getUserData(int userId)
        {
            dynamic returnModel = new System.Dynamic.ExpandoObject();
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

        [HttpPost]
        public async Task<IActionResult> updateVehicleUserpPerm(string targerMail, string vehId, eVehiclePermissions perm)
        {
            if (!_SQL.vehicles.Any(v => v.chassis_number == vehId)) //Nem a létezik a jármű
            {
                _Notification.AddErrorToastMessage("A keresett jármű nem található!");
                return NotFound("A keresett jármű nem található!");
            }

            if (_SQL.users.Any(u => u.email == targerMail))
            {
                int id = _SQL.users.SingleOrDefault(tmp=>tmp.email == targerMail).id;
                if(_SQL.vehiclePermissions.Any(tmp=>tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.USER && tmp.target_id == id)) {
                    VehiclePermission vp = _SQL.vehiclePermissions.Single(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.USER && tmp.target_id == id);
                    if(vp.permission == perm)
                    {
                        _Notification.AddInfoToastMessage("Nem történt módosítás.");
                        return Ok("Nem történt módosítás.");
                    }
                    else
                    {
                        if(perm != eVehiclePermissions.NONE)
                        {
                            vp.permission = perm;
                            _SQL.vehiclePermissions.Update(vp);
                        }
                        else
                            _SQL.vehiclePermissions.Remove(vp);
                        
                        _SQL.SaveChanges();
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                        return Ok("Sikeres módosítás!");
                    }
                }
                else {
                    if (perm != eVehiclePermissions.NONE)
                    {
                        _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = perm, target_id = id, target_type = eVehicleTargetTypes.USER, vehicle_id = vehId });
                        _SQL.SaveChanges();
                    }
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!");
                    return Ok("Jogosultság rögzítve!");
                }
            }
            else if (_SQL.services.Any(u => u.email == targerMail))
            {
                int id = _SQL.services.SingleOrDefault(tmp => tmp.email == targerMail).id;
                if (_SQL.vehiclePermissions.Any(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.SERVICE && tmp.target_id == id))
                {
                    VehiclePermission vp = _SQL.vehiclePermissions.Single(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.SERVICE && tmp.target_id == id);
                    if (vp.permission == perm)
                    {
                        _Notification.AddInfoToastMessage("Nem történt módosítás.");
                        return Ok("Nem történt módosítás.");
                    }
                    else
                    {
                        if (perm != eVehiclePermissions.NONE)
                        {
                            vp.permission = perm;
                            _SQL.vehiclePermissions.Update(vp);
                        }
                        else
                            _SQL.vehiclePermissions.Remove(vp);

                        _SQL.SaveChanges();
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                        return Ok("Sikeres módosítás!");
                    }
                }
                else
                {
                    if (perm != eVehiclePermissions.NONE)
                    {
                        _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = perm, target_id = id, target_type = eVehicleTargetTypes.SERVICE, vehicle_id = vehId });
                        _SQL.SaveChanges();
                    }
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!");
                    return Ok("Jogosultság rögzítve!");
                }
            }
            else if (_SQL.dealers.Any(u => u.email == targerMail))
            {
                int id = _SQL.dealers.SingleOrDefault(tmp => tmp.email == targerMail).id;
                if (_SQL.vehiclePermissions.Any(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.DEALER && tmp.target_id == id))
                {
                    VehiclePermission vp = _SQL.vehiclePermissions.Single(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.DEALER && tmp.target_id == id);
                    if (vp.permission == perm)
                    {
                        _Notification.AddInfoToastMessage("Nem történt módosítás.");
                        return Ok("Nem történt módosítás.");
                    }
                    else
                    {
                        if (perm != eVehiclePermissions.NONE)
                        {
                            vp.permission = perm;
                            _SQL.vehiclePermissions.Update(vp);
                        }
                        else
                            _SQL.vehiclePermissions.Remove(vp);
                        _SQL.SaveChanges();
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                        return Ok("Sikeres módosítás!");
                    }
                }
                else
                {
                    if (perm != eVehiclePermissions.NONE)
                    {
                        _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = perm, target_id = id, target_type = eVehicleTargetTypes.DEALER, vehicle_id = vehId });
                        _SQL.SaveChanges();
                    }
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!");
                    return Ok("Jogosultság rögzítve!");
                }
            }
            else if (_SQL.factories.Any(u => u.email == targerMail))
            {
                int id = _SQL.factories.SingleOrDefault(tmp => tmp.email == targerMail).id;
                if (_SQL.vehiclePermissions.Any(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.FACTORY && tmp.target_id == id))
                {
                    VehiclePermission vp = _SQL.vehiclePermissions.Single(tmp => tmp.vehicle_id == vehId && tmp.target_type == eVehicleTargetTypes.FACTORY && tmp.target_id == id);
                    if (vp.permission == perm)
                    {
                        _Notification.AddInfoToastMessage("Nem történt módosítás.");
                        return Ok("Nem történt módosítás.");
                    }
                    else
                    {
                        if (perm != eVehiclePermissions.NONE)
                        {
                            vp.permission = perm;
                            _SQL.vehiclePermissions.Update(vp);
                        }
                        else
                            _SQL.vehiclePermissions.Remove(vp);
                        _SQL.SaveChanges();
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                        return Ok("Sikeres módosítás!");
                    }
                }
                else
                {
                    if (perm != eVehiclePermissions.NONE)
                    {
                        _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = perm, target_id = id, target_type = eVehicleTargetTypes.FACTORY, vehicle_id = vehId });
                        _SQL.SaveChanges();
                    }
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!");
                    return Ok("Jogosultság rögzítve!");
                }
            }
            else
            {
                _Notification.AddErrorToastMessage("A keresett felhasználó nem található!");
                return NotFound("A keresett felhasználó nem található!");
            }
        }
        #endregion

        #region ServiceRegion
        [HttpGet]
        public async Task<IActionResult> getServiceData(int serviceId)
        {
            dynamic returnModel = new System.Dynamic.ExpandoObject();
            Service findService = _SQL.services.SingleOrDefault(u => u.id == serviceId);

            if (findService == null)
            {
                _Notification.AddErrorToastMessage("A keresett szerviz nem található!");
                return BadRequest();
            }

            List<UserVehicle> vehicles = new List<UserVehicle>();
            foreach (var item in _SQL.vehiclePermissions.Where(i => i.target_type == eVehicleTargetTypes.SERVICE && i.target_id == serviceId))
            {
                using (SQL mysql = new SQL())
                {
                    vehicles.Add(new UserVehicle { p = item.permission, v = mysql.vehicles.SingleOrDefault(vh => vh.chassis_number == item.vehicle_id) });
                }
            }

            findService.password = null;

            returnModel.Service = findService;
            returnModel.ServiceVehicles = vehicles;

            return Ok(returnModel);
        }

        [HttpPost]
        public async Task<IActionResult> updateServiceData([FromBody] AdminUpdateServiceDataModel m)
        {
            if (ModelState.IsValid)
            {
                if (_SQL.services.Any(u => u.id == m.id))
                {
                    Service u = _SQL.services.Single(usr => usr.id == m.id);
                    if (u.email != m.email)
                        u.email = m.email;
                    if (u.name != m.name)
                        u.name = m.name;
                    if (u.status != m.status)
                        u.status = m.status;
                    if(u.phone != m.phone)
                        u.phone = m.phone;
                    if(u.description != m.description)
                        u.description = m.description;
                    if(u.country != m.country)
                        u.country = m.country;
                    if(u.city != m.city)
                        u.city = m.city;
                    if(u.address != m.address)
                        u.address = m.address;
                    if(u.website != m.website)
                        u.website = m.website;

                    _SQL.services.Update(u);
                    _SQL.SaveChanges();

                    _Notification.AddSuccessToastMessage("Módosítások mentése sikeres!");
                    return Ok("Módosítások sikeresen elmentve!");
                }
                else
                {
                    _Notification.AddErrorToastMessage("Sikertelen mentés: Szerviz nem található!");
                    return NotFound("A szerviz nem található!");
                }
            }
            else
            {
                _Notification.AddErrorToastMessage("Módosítás sikertelen: Helytelen adatok!");
                return BadRequest("Hibás adatok!");
            }

        }
        #endregion

        #region vehicleRegion

        [HttpPost]
        public async Task<IActionResult> deleteFuelingData(string fuelingId)
        {
            if(_SQL.refuels.Any(rf=>rf.id == Guid.Parse(fuelingId)))
            {
                Refuel refu = _SQL.refuels.Single(rf => rf.id == Guid.Parse(fuelingId));
                _SQL.refuels.Remove(refu);
                _SQL.SaveChanges();
                _Notification.AddSuccessToastMessage("Sikeres törlés!");
                return Ok("Sikeres törlés!");
            }
            else
            {
                _Notification.AddErrorToastMessage("A keresett tankolás nem található!");
                return NotFound("A keresett tankolás nem található!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> deleteServiceEventData(string serviceEventId)
        {
            if (_SQL.serviceEvents.Any(se => se.id == Guid.Parse(serviceEventId)))
            {
                ServiceEvent sev = _SQL.serviceEvents.Single(se => se.id == Guid.Parse(serviceEventId));
                _SQL.serviceEvents.Remove(sev);
                _SQL.SaveChanges();
                _Notification.AddSuccessToastMessage("Sikeres törlés!");
                return Ok("Sikeres törlés!");
            }
            else
            {
                _Notification.AddSuccessToastMessage("A keresett szerviz nem található!");
                return NotFound("A keresett szerviz nem található!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> deleteMileageData(string mileageId)
        {
            if (_SQL.mileageStands.Any(ms => ms.id == Guid.Parse(mileageId)))
            {
                MileageStand mst = _SQL.mileageStands.Single(ms => ms.id == Guid.Parse(mileageId));
                _SQL.mileageStands.Remove(mst);
                _SQL.SaveChanges();
                _Notification.AddSuccessToastMessage("Sikeres törlés!");
                return Ok("Sikeres törlés!");
            }
            else
            {
                _Notification.AddErrorToastMessage("A keresett adat nem található!");
                return NotFound("A keresett adat nem található!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> deleteOtherCostData(string costId)
        {
            if (_SQL.otherCosts.Any(oc => oc.id == Guid.Parse(costId)))
            {
                OtherCost otc = _SQL.otherCosts.Single(oc => oc.id == Guid.Parse(costId));
                _SQL.otherCosts.Remove(otc);
                _SQL.SaveChanges();
                _Notification.AddSuccessToastMessage("Sikeres törlés!");
                return Ok("Sikeres törlés!");
            }
            else
            {
                _Notification.AddErrorToastMessage("A keresett adat nem található!");
                return NotFound("A keresett adat nem található!");
            }
        }

        /*[HttpPost]
        public async Task<IActionResult> addMileageStand(string vehicleId, int mileage, DateTime date)
        {

        }*/

        [HttpPost]
        public async Task<IActionResult> addServiceEvent([FromBody] AddServiceEventAdminMondel serviceData)
        {
            if (ModelState.IsValid)
            {
                if (!_SQL.vehicles.Any(v => v.chassis_number == serviceData.vehicleId))
                {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: A keresett jármű nem szerepel a rendszerben.");
                    return BadRequest("A jármű nem található!");
                }
                if (serviceData.newServiceCost == null || serviceData.newServiceMileage == null)
                {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: Hibás numerikus mezők.");
                    return BadRequest("Hibás beviteli mezők!");
                }

                if(!_SQL.services.Any(s=>s.id == serviceData.newServiceService)) {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: Hibás szerviz.");
                    return BadRequest("Hibás szerviz!");
                }

                _SQL.serviceEvents.Add(new ServiceEvent()
                {
                    service_id = (int)serviceData.newServiceService,
                    serviceType = serviceData.newServiceType,
                    comment = serviceData.newServiceOther,
                    cost = (int)serviceData.newServiceCost,
                    date = serviceData.newServiceDate,
                    description = serviceData.newServiceDescription,
                    mileage = (int)serviceData.newServiceMileage,
                    title = serviceData.newServiceTitle,
                    vehicle_id = serviceData.vehicleId,
                    id = new Guid()
                });

                _SQL.SaveChanges();

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!");
                return Ok("Sikeres rögzítés!");
            }
            else
            {
                _Notification.AddErrorToastMessage("Sikertelen rögzítés: Adathiba!");
                return BadRequest("Adat hiba!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> addOtherCost([FromBody] AddNewCostModel m)
        {
            if (ModelState.IsValid)
            {
                if (!_SQL.vehicles.Any(v => v.chassis_number == m.vehicle_id))
                {
                    _Notification.AddErrorToastMessage("Jármű nem található!");
                    return NotFound();
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

        [HttpPost]
        public async Task<IActionResult> addRefuelingData([FromBody] AddNewRefuelModel m)
        {
            if (ModelState.IsValid)
            {
                if (!_SQL.vehicles.Any(v => v.chassis_number == m.vehicle_id))
                {
                    _Notification.AddErrorToastMessage("Jármű nem található!");
                    return NotFound();
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

        #endregion
    }
}

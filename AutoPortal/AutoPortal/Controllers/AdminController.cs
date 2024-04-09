using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using AutoPortal.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            ViewBag.Factories = _SQL.factories.ToList();
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

        public IActionResult sendMail()
        {
            return View();
        }

        public IActionResult vehicleHandler(string vehicleId)
        {
            Vehicle v = _SQL.vehicles.SingleOrDefault(ve => ve.chassis_number == vehicleId);

            if (v == null) //Nincs ilyen jármű
                return NotFound();

            List<MileageStandModel> mileageStands = v.getMileageStands();

            ViewBag.Refuels = Refuel.GetVehicleRefuelsAdmin(vehicleId);
            ViewBag.OtherCosts = OtherCost.GetVehicleOtherCostsAdmin(vehicleId);
            ViewBag.MileageStands = mileageStands.OrderBy(tmp => tmp.RecordedDate).ToList();
            ViewBag.Vehicle = v;
            ViewBag.ServiceEvents = _SQL.serviceEvents.Where(tmp=>tmp.vehicle_id == vehicleId).ToList();

            ViewBag.vehicleCategories = _SQL.vehicleCategories.ToList();
            ViewBag.fuels = _SQL.fuelTypes.ToList();
            ViewBag.transmissions = _SQL.transmissionTypes.ToList();
            ViewBag.drives = _SQL.driveTypes.ToList();
            ViewBag.bodyTypes = _SQL.bodyTypes.ToList();

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
                    _Notification.AddSuccessToastMessage("Módosítások mentése sikeres!", new ToastrOptions() { Title = "Siker" });
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

            if(uType != eVehicleTargetTypes.DEALER && perm == eVehiclePermissions.DEALER)
            {
                _Notification.AddErrorToastMessage("Kereskedői jogosultság csak kereskedői fióknak adható!");
                return BadRequest("Kereskedői jogosultság csak kereskedői fióknak adható!");
            }

            if (_SQL.vehiclePermissions.Any(vp => vp.target_type == uType && vp.target_id == uid && vp.vehicle_id == vehId))
            {
                if (perm == eVehiclePermissions.NONE)
                {
                    VehiclePermission toRemove = _SQL.vehiclePermissions.Single(vp => vp.target_type == uType && vp.target_id == uid && vp.vehicle_id == vehId);
                    _SQL.vehiclePermissions.Remove(toRemove);
                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
                    return Ok("Sikeres módosítás!");
                }
                else
                {
                    VehiclePermission toUpdate = _SQL.vehiclePermissions.Single(vp => vp.target_type == uType && vp.target_id == uid && vp.vehicle_id == vehId);
                    toUpdate.permission = perm;
                    _SQL.vehiclePermissions.Update(toUpdate);
                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
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
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
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
                if(perm == eVehiclePermissions.DEALER)
                {
                    _Notification.AddWarningToastMessage("A megadott fiók nem kereskedői.", new ToastrOptions() { Title = "Figyelmeztetés" });
                    return BadRequest("A megadott fiók nem kereskedői.");
                }
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
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
                        return Ok("Sikeres módosítás!");
                    }
                }
                else {
                    if (perm != eVehiclePermissions.NONE)
                    {
                        _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = perm, target_id = id, target_type = eVehicleTargetTypes.USER, vehicle_id = vehId });
                        _SQL.SaveChanges();
                    }
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!", new ToastrOptions() { Title = "Siker" });
                    return Ok("Jogosultság rögzítve!");
                }
            }
            else if (_SQL.services.Any(u => u.email == targerMail))
            {
                if (perm == eVehiclePermissions.DEALER)
                {
                    _Notification.AddWarningToastMessage("A megadott fiók nem kereskedői.", new ToastrOptions() { Title = "Figyelmeztetés" });
                    return BadRequest("A megadott fiók nem kereskedői.");
                }
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
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
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
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!", new ToastrOptions() { Title = "Siker" });
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
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
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
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!", new ToastrOptions() { Title = "Siker" });
                    return Ok("Jogosultság rögzítve!");
                }
            }
            else if (_SQL.factories.Any(u => u.email == targerMail))
            {
                if (perm == eVehiclePermissions.DEALER)
                {
                    _Notification.AddWarningToastMessage("A megadott fiók nem kereskedői.", new ToastrOptions() { Title = "Figyelmeztetés" });
                    return BadRequest("A megadott fiók nem kereskedői.");
                }
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
                        _Notification.AddSuccessToastMessage("Sikeres módosítás!", new ToastrOptions() { Title = "Siker" });
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
                    _Notification.AddSuccessToastMessage("Jogosultság rögzítve!", new ToastrOptions() { Title = "Siker" });
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

            List<Review> reviews = _SQL.reviews.Where(r => r.target_type == eVehicleTargetTypes.SERVICE && r.target_id == serviceId).ToList();
            reviews.ForEach(r => {
                r.LoadReviewWriter();
            });
            returnModel.Reviews = reviews;

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

                    _Notification.AddSuccessToastMessage("Módosítások mentése sikeres!", new ToastrOptions() { Title = "Siker" });
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

        #region DealerRegion
        [HttpGet]
        public async Task<IActionResult> getDealerData(int dealerId)
        {
            dynamic returnModel = new System.Dynamic.ExpandoObject();
            Dealer findDealer = _SQL.dealers.SingleOrDefault(u => u.id == dealerId);

            if (findDealer == null)
            {
                _Notification.AddErrorToastMessage("A keresett kereskedő nem található!");
                return BadRequest();
            }

            List<UserVehicle> vehicles = new List<UserVehicle>();
            foreach (var item in _SQL.vehiclePermissions.Where(i => i.target_type == eVehicleTargetTypes.DEALER && i.target_id == dealerId))
            {
                using (SQL mysql = new SQL())
                {
                    vehicles.Add(new UserVehicle { p = item.permission, v = mysql.vehicles.SingleOrDefault(vh => vh.chassis_number == item.vehicle_id) });
                }
            }

            findDealer.password = null;

            returnModel.Dealer = findDealer;
            returnModel.DealerVehicles = vehicles;

            List<Review> reviews = _SQL.reviews.Where(r => r.target_type == eVehicleTargetTypes.DEALER && r.target_id == dealerId).ToList();
            reviews.ForEach(r => {
                r.LoadReviewWriter();
            });
            returnModel.Reviews = reviews;

            return Ok(returnModel);
        }

        [HttpPost]
        public async Task<IActionResult> updateDealerData([FromBody] AdminUpdateDealerDataModel m)
        {
            if (ModelState.IsValid)
            {
                if (_SQL.dealers.Any(u => u.id == m.id))
                {
                    Dealer u = _SQL.dealers.Single(usr => usr.id == m.id);
                    if (u.email != m.email)
                        u.email = m.email;
                    if (u.name != m.name)
                        u.name = m.name;
                    if (u.status != m.status)
                        u.status = m.status;
                    if (u.phone != m.phone)
                        u.phone = m.phone;
                    if (u.description != m.description)
                        u.description = m.description;
                    if (u.country != m.country)
                        u.country = m.country;
                    if (u.city != m.city)
                        u.city = m.city;
                    if (u.address != m.address)
                        u.address = m.address;
                    if (u.website != m.website)
                        u.website = m.website;

                    _SQL.dealers.Update(u);
                    _SQL.SaveChanges();

                    _Notification.AddSuccessToastMessage("Módosítások mentése sikeres!", new ToastrOptions() { Title = "Siker" });
                    return Ok("Módosítások sikeresen elmentve!");
                }
                else
                {
                    _Notification.AddErrorToastMessage("Sikertelen mentés: Kereskedő nem található!");
                    return NotFound("A kereskedő nem található!");
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
                _Notification.AddSuccessToastMessage("Sikeres törlés!", new ToastrOptions() { Title = "Siker" });
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
                _Notification.AddSuccessToastMessage("Sikeres törlés!", new ToastrOptions() { Title = "Siker" });
                return Ok("Sikeres törlés!");
            }
            else
            {
                _Notification.AddSuccessToastMessage("A keresett szerviz nem található!", new ToastrOptions() { Title = "Siker" });
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
                _Notification.AddSuccessToastMessage("Sikeres törlés!", new ToastrOptions() { Title = "Siker" });
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
                _Notification.AddSuccessToastMessage("Sikeres törlés!", new ToastrOptions() { Title = "Siker" });
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

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!", new ToastrOptions() { Title = "Siker" });
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

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!", new ToastrOptions() { Title = "Siker" });
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

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!", new ToastrOptions() { Title = "Siker" });
                return Ok();
            }
            _Notification.AddErrorToastMessage("Hiányos adatok!");
            return BadRequest();
        }

        #endregion

        #region FACTORY_REGION

        [HttpPost]
        public async Task<IActionResult> registerFactory(string email, string name, eAccountStatus? status)
        {
            if(!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(name) && status != null)
            {
                if(_SQL.users.Any(u => u.email == email) || _SQL.services.Any(s => s.email == email) || _SQL.dealers.Any(d => d.email == email) || _SQL.factories.Any(f => f.email == email))
                {
                    _Notification.AddErrorToastMessage("Az email cím már használatban van!");
                    return BadRequest("Az email cím már használatban van!");
                }

                string randomPassword = PasswordManager.GenerateRandomPassword();

                Factory f = new Factory()
                {
                    email = email,
                    name = name,
                    status = (eAccountStatus)status,
                    password = PasswordManager.GenerateHash(randomPassword)
                };
                _SQL.factories.Add(f);
                await _SQL.SaveChangesAsync();
                await MailSender.SendFactoryRegisterMail(email, name, randomPassword);
                _Notification.AddSuccessToastMessage("Gyártó rögzítés sikeres!", new ToastrOptions { Title = "Siker" });
                f.password = null;
                return Ok(JsonConvert.SerializeObject(f));
            }
            else
            {
                _Notification.AddErrorToastMessage("Hibás adatok!");
                return BadRequest("Hibás adatok!");
            }
        }

        [HttpGet]
        public async Task<IActionResult> getFactoryData(int factoryId)
        {
            dynamic returnModel = new System.Dynamic.ExpandoObject();
            Factory findFactory = _SQL.factories.SingleOrDefault(u => u.id == factoryId);

            if (factoryId == null)
            {
                _Notification.AddErrorToastMessage("A keresett gyártó nem található!");
                return BadRequest();
            }

            List<UserVehicle> vehicles = new List<UserVehicle>();
            foreach (var item in _SQL.vehiclePermissions.Where(i => i.target_type == eVehicleTargetTypes.FACTORY && i.target_id == factoryId))
            {
                using (SQL mysql = new SQL())
                {
                    vehicles.Add(new UserVehicle { p = item.permission, v = mysql.vehicles.SingleOrDefault(vh => vh.chassis_number == item.vehicle_id) });
                }
            }

            findFactory.password = null;

            returnModel.Factory = findFactory;
            returnModel.FactoryVehicles = vehicles;

            return Ok(returnModel);
        }

        [HttpPost]
        public async Task<IActionResult> updateFactoryData([FromBody] AdminUpdateFactoryDataModel m)
        {
            if (ModelState.IsValid)
            {
                if (_SQL.factories.Any(u => u.id == m.id))
                {
                    Factory u = _SQL.factories.Single(usr => usr.id == m.id);
                    if (u.email != m.email)
                        u.email = m.email;
                    if (u.name != m.name)
                        u.name = m.name;
                    if (u.status != m.status)
                        u.status = m.status;

                    _SQL.factories.Update(u);
                    _SQL.SaveChanges();

                    _Notification.AddSuccessToastMessage("Módosítások mentése sikeres!", new ToastrOptions() { Title = "Siker" });
                    return Ok("Módosítások sikeresen elmentve!");
                }
                else
                {
                    _Notification.AddErrorToastMessage("Sikertelen mentés: gyár nem található!");
                    return NotFound("A gyár nem található!");
                }
            }
            else
            {
                _Notification.AddErrorToastMessage("Módosítás sikertelen: Helytelen adatok!");
                return BadRequest("Hibás adatok!");
            }

        }

        [HttpPost]
        public async Task<IActionResult> askNewFactoryPassword(int factoryId)
        {
            if(_SQL.factories.Any(f=>f.id == factoryId))
            {
                string newPwd = PasswordManager.GenerateRandomPassword();
                Factory f = _SQL.factories.Single(f => f.id == factoryId);
                f.password = PasswordManager.GenerateHash(newPwd);
                _SQL.factories.Update(f);
                await _SQL.SaveChangesAsync();
                await MailSender.SendAdminNewFactoryPwdMail(f.email, f.name, newPwd);
                _Notification.AddSuccessToastMessage("Sikeres új jelszó igénylés!", new ToastrOptions() { Title = "Siker" });
                return Ok("Sikeres új jelszó igénylés!");
            }
            else
            {
                _Notification.AddErrorToastMessage("A gyár nem található!");
                return BadRequest("A gyár nem található!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendAdminMail(int mailtoCategory, string subject, string body)
        {
            if (mailtoCategory < 1 || mailtoCategory > 5)
            {
                _Notification.AddErrorToastMessage("Levél küldés sikertelen: célcsoport hiba!");
                return BadRequest("Levél küldés sikertelen: célcsoport hiba!");
            }
            List<string> mails = new();
            switch (mailtoCategory)
            {
                case 1: //Felhasználó
                    mails = _SQL.users.Select(u => u.email).ToList();
                    break;
                case 2: //Szerviz
                    mails = _SQL.services.Select(s => s.email).ToList();
                    break;
                case 3: //Kereskedő
                    mails = _SQL.dealers.Select(d => d.email).ToList();
                    break;
                case 4: //Gyártó
                    mails = _SQL.factories.Select(f => f.email).ToList();
                    break;
                case 5: //Mindenki
                    mails.AddRange(_SQL.users.Select(u => u.email).ToList());
                    mails.AddRange(_SQL.services.Select(s => s.email).ToList());
                    mails.AddRange(_SQL.dealers.Select(d => d.email).ToList());
                    mails.AddRange(_SQL.factories.Select(f => f.email).ToList());
                    break;
            }

            try
            {
                foreach (string mail in mails)
                {
                    MailModel m = new MailModel()
                    {
                        to = mail,
                        body = body,
                        subject = subject,
                        from = "admin@AutoPortal.hu",
                        isHtml = true,
                    };
                    MailSender.SendMail(m);
                }
                _Notification.AddSuccessToastMessage($"Körléevél küldés sikeresen beütemezve {mails.Count} felhasználó számára!");
                return Ok($"Körléevél küldés sikeresen beütemezve {mails.Count} felhasználó számára!");
            }
            catch
            {
                _Notification.AddErrorToastMessage("Körléevél küldése sikertelen!");
                return BadRequest("Körléevél küldése sikertelen!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(Guid ReviewId)
        {
            if(_SQL.reviews.Any(r=>r.id == ReviewId))
            {
                Review rev = _SQL.reviews.Single(r => r.id == ReviewId);
                _SQL.reviews.Remove(rev);
                await _SQL.SaveChangesAsync();
                _Notification.AddSuccessToastMessage("Sikeres véleménytörlés!");
                return Ok("Sikeres véleménytörlés!");
            }
            else {
                _Notification.AddErrorToastMessage("A keresett vélemény nem található!");
                return NotFound("A keresett vélemény nem található!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> newVehicleOwner(string vehicleId, string newOwnerMail)
        {
            eVehicleTargetTypes userType;
            int newUserId;
            if (_SQL.users.Any(u => u.email == newOwnerMail))
            {
                userType = eVehicleTargetTypes.USER;
                newUserId = _SQL.users.Single(u => u.email == newOwnerMail).id;
            }
            else if (_SQL.services.Any(s => s.email == newOwnerMail))
            {
                userType = eVehicleTargetTypes.SERVICE;
                newUserId = _SQL.services.Single(s => s.email == newOwnerMail).id;
            }
            else if (_SQL.dealers.Any(d => d.email == newOwnerMail))
            {
                userType = eVehicleTargetTypes.DEALER;
                newUserId = _SQL.dealers.Single(d => d.email == newOwnerMail).id;
            }
            else if (_SQL.factories.Any(f => f.email == newOwnerMail))
            {
                _Notification.AddErrorToastMessage("Gyártó számára nem írható át a jogviszony!");
                return BadRequest("Gyártó számára nem írható át a jogviszony!");
            }
            else
            {
                _Notification.AddErrorToastMessage("A keresett felhasználó nem található!");
                return BadRequest("A keresett felhasználó nem található!");
            }

            Vehicle v = _SQL.vehicles.Single(v => v.chassis_number == vehicleId);

            List<Refuel> refuels = _SQL.refuels.Where(r => r.vehicle_id == v.chassis_number).ToList();
            List<OtherCost> otherCosts = _SQL.otherCosts.Where(r => r.vehicle_id == v.chassis_number).ToList();
            List<ServiceEvent> serviceEvents = _SQL.serviceEvents.Where(r => r.vehicle_id == v.chassis_number).ToList();

            foreach (Refuel rf in refuels)
            {
                rf.archive = true;
                _SQL.refuels.Update(rf);
            }
            foreach (OtherCost oc in otherCosts)
            {
                oc.archive = true;
                _SQL.otherCosts.Update(oc);
            }
            foreach (ServiceEvent se in serviceEvents)
            {
                se.archive = true;
                _SQL.serviceEvents.Update(se);
            }

            List<VehiclePermission> permissions = _SQL.vehiclePermissions.Where(vp => vp.vehicle_id == vehicleId).ToList();

            if (_SQL.vehicleSales.Any(vs => vs.vehicle_id == vehicleId))
            {
                SaleVehicle sv = _SQL.vehicleSales.Single(vs => vs.vehicle_id == vehicleId);
                _SQL.vehicleSales.Remove(sv);
            }
            _SQL.vehiclePermissions.RemoveRange(permissions);

            _SQL.vehiclePermissions.Add(new VehiclePermission() { permission = eVehiclePermissions.OWNER, target_id = newUserId, target_type = userType, vehicle_id = vehicleId });
            _SQL.vehicleOwnerChanges.Add(new VehicleOwnerChange() { id = Guid.NewGuid(), new_owner = newUserId, owner_type = userType, owner_change_date = DateTime.Now, vehicle_id = vehicleId });

            await _SQL.SaveChangesAsync();

            _Notification.AddSuccessToastMessage("Sikeres tulajváltás!");
            return Ok("Sikeres tulajváltás!");
        }

        [HttpPost]
        public async Task<IActionResult> updateVehicleDetails([FromBody]AdminUpdateVehicleModel model)
        {
            if (ModelState.IsValid)
            {
                if (_SQL.vehicles.Any(v => v.chassis_number == model.chassis_number))
                {
                    Vehicle veh = _SQL.vehicles.Single(v => v.chassis_number == model.chassis_number);
                    veh.license = model.license;
                    veh.make = model.make;
                    veh.model = model.model;
                    veh.modeltype = model.modeltype;
                    veh.manufact_year = model.manufactyear;
                    veh.category = model.category;
                    veh.body = model.body;
                    veh.num_of_doors = model.numofdoors;
                    veh.num_of_seats = model.numofseats;
                    veh.weight = model.weight;
                    veh.max_weight = model.maxweight;
                    veh.engine_number = model.enginenumber;
                    veh.engine_code = model.enginecode;
                    veh.fuel = model.fueltype;
                    veh.engine_ccm = model.engineccm;
                    veh.performance = model.performance;
                    veh.torque = model.torque;
                    veh.transmission = model.transmissiontype;
                    veh.num_of_gears = model.numofgears;
                    veh.drive = model.drivetype;

                    _SQL.vehicles.Update(veh);
                    await _SQL.SaveChangesAsync();

                    _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                    return Ok("Sikeres módosítás!");
                }
                else
                {
                    _Notification.AddErrorToastMessage("A keresett jármű nem található!");
                    return NotFound("A keresett jármű nem található!");
                }
                
            }
            else
            {
                _Notification.AddErrorToastMessage("Hibás adatok!");
                return BadRequest("Hibás adatok!");
            }
            
        }
        #endregion
    }
}

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

            VehiclePermission vp = _SQL.vehiclePermissions.SingleOrDefault(p => p.target_type == loginType && p.target_id == loginId && p.vehicle_id == vehicleId);

            if (vp == null) //Nincs semmilyen jogosultsága a felhasználónak a járműhöz
                return Forbid();

            List<MileageStandModel> mileageStands = v.getMileageStands();

            ViewBag.Refuels = Refuel.GetVehicleRefuels(vehicleId);
            ViewBag.OtherCosts = OtherCost.GetVehicleOtherCosts(vehicleId);
            ViewBag.MileageStands = mileageStands.OrderBy(tmp => tmp.RecordedDate).ToList();
            ViewBag.ServiceEvents = _SQL.serviceEvents.Where(tmp => tmp.vehicle_id == vehicleId).ToList();

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

        public IActionResult pastServices()
        {
            int sid = ((Service)user).id;
            ViewBag.ServiceEvents = _SQL.serviceEvents.Where(se => se.service_id == sid).ToList();
            return View();
        }

        public IActionResult createVehicleSale()
        {
            List<Vehicle> vehicles = new();
            List<VehiclePermission> uv = _SQL.vehiclePermissions.Where(p=>p.target_type == loginType && p.target_id == loginId).ToList();
            foreach(VehiclePermission vp in uv)
            {
                vehicles.Add(_SQL.vehicles.Single(v => v.chassis_number == vp.vehicle_id));
            }
            ViewBag.vehicles = vehicles;
            return View();
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

        [HttpPost]
        public async Task<IActionResult> addServiceEvent(AddServiceEventModel serviceData)
        {
            if(ModelState.IsValid)
            {
                if (!((Service)user).isValid())
                {
                    _Notification.AddErrorToastMessage("Nincs jogosultsága rögzíteni! A szerviz inaktív, vagy ki lett tiltva.");
                    return View();
                }
                if(!_SQL.vehicles.Any(v=>v.chassis_number == serviceData.vehicleId))
                {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: A keresett jármű nem szerepel a rendszerben.");
                    return View();
                }
                if(serviceData.serviceCost == null || serviceData.serviceMileage == null)
                {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: Hibás numerikus mezők.");
                    return View();
                }

                if(serviceData.serviceDate <= DateTime.Now.AddDays(-2).AddHours(-1))//További -1 óra, mivel kliensolalon nem frissül mp-enként a min => Eltelhet x idő mire elküldi ténylegesen
                {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: Legfeljebb 48 órával nappal visszamenőleg lehet szerviz eseményt felvenni. Kérjük vegye fel a kapcsolatot egy adminnal.");
                    return View();
                }

                if(serviceData.serviceDate > DateTime.Now.AddDays(1))
                {
                    _Notification.AddErrorToastMessage("Sikertelen rögzítés: Legfeljebb 24 órával előre lehet szerviz eseményt felvenni.");
                    return View();
                }

                _SQL.serviceEvents.Add(new ServiceEvent() { 
                    service_id = ((Service)user).id,
                    serviceType = serviceData.serviceType,
                    comment = serviceData.serviceComment,
                    cost = (int)serviceData.serviceCost,
                    date = serviceData.serviceDate,
                    description = serviceData.serviceDescription,
                    mileage = (int)serviceData.serviceMileage,
                    title = serviceData.serviceTitle,
                    vehicle_id = serviceData.vehicleId,
                    id = new Guid()
                });

                _SQL.SaveChanges();

                _Notification.AddSuccessToastMessage("Sikeres rögzítés!");
                return View();
            }
            else{
                _Notification.AddErrorToastMessage("Sikertelen rögzítés: Adathiba!");
                return View();
            }
            
        }

        [HttpGet]
        public async Task<IActionResult> getServiceEventDetails(string serviceEventId)
        {
            if (!_SQL.serviceEvents.Any(se => se.id == Guid.Parse(serviceEventId)))
            {
                _Notification.AddErrorToastMessage("A keresett azonosító alatt nem található szerviz esemény!");
                return NotFound();
            }
            if (_SQL.serviceEvents.SingleOrDefault(se => se.id == Guid.Parse(serviceEventId)).service_id != ((Service)user).id)
            {
                _Notification.AddErrorToastMessage("Nincs jogosultsága a művelethez!");
                return Forbid();
            }
            dynamic returnModel = new System.Dynamic.ExpandoObject();
            returnModel.ServiceEvent = _SQL.serviceEvents.SingleOrDefault(se => se.id == Guid.Parse(serviceEventId));
            string chassis = returnModel.ServiceEvent.vehicle_id;
            Vehicle veh = _SQL.vehicles.Single(v=>v.chassis_number == chassis);
            returnModel.Vehicle_Make = veh.make;
            returnModel.Vehicle_Model = veh.model;
            returnModel.Vehicle_ModelType = veh.modeltype;
            returnModel.Vehicle_Manufact_year = veh.manufact_year;
            returnModel.Vehicle_Chassis_number = veh.chassis_number;

            return Ok(returnModel);
        }

        [HttpPost]
        public async Task<IActionResult> deleteServiceEvent(string serviceEventId)
        {
            if(!_SQL.serviceEvents.Any(se=>se.id == Guid.Parse(serviceEventId)))
            {
                _Notification.AddErrorToastMessage("A keresett azonosító alatt nem található szerviz esemény!");
                return NotFound();
            }
            if (_SQL.serviceEvents.SingleOrDefault(se => se.id == Guid.Parse(serviceEventId)).service_id != ((Service)user).id)
            {
                _Notification.AddErrorToastMessage("Nincs jogosultsága a művelethez!");
                return Forbid();
            }
            ServiceEvent se = _SQL.serviceEvents.Single(e => e.id == Guid.Parse(serviceEventId));
            _SQL.serviceEvents.Remove(se);
            _SQL.SaveChanges();
            _Notification.AddSuccessToastMessage("Sikerese törlés!");
            return Ok();
        }
        #endregion
    }
}

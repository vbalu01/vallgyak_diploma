using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using AutoPortal.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using System.Collections.Generic;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace AutoPortal.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        public UserController(IConfiguration config, SQL sql, IToastNotification notification, IWebHostEnvironment environment) : base(config, sql, notification, environment)
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

        public IActionResult myVehicleSales()
        {
            List<VehiclePermission> userVehicles = _SQL.vehiclePermissions.Where(vp => vp.target_id == loginId && vp.target_type == loginType).ToList();
            List<VehicleSaleModel> vehicles = new();

            foreach (VehiclePermission vp in userVehicles)
            {
                
                if (_SQL.vehicleSales.Any(vs=>vs.vehicle_id == vp.vehicle_id))
                {
                    vehicles.Add(new VehicleSaleModel() {
                        Vehicle = _SQL.vehicles.Single(v => v.chassis_number == vp.vehicle_id),
                        SaleVehicle = _SQL.vehicleSales.Single(vs => vs.vehicle_id == vp.vehicle_id),
                        images = Directory.GetFiles(_Environment.WebRootPath + "/Images/SaleImages/" + _SQL.vehicleSales.Single(vs => vs.vehicle_id == vp.vehicle_id).transaction_id + "/").Select(f => Path.GetFileName(f)).ToList()
                });
                }
            }
            ViewBag.vehicles = vehicles;
            return View();
        }

        public IActionResult findService()
        {
            ViewBag.Services = _SQL.services.Where(d => d.status.HasFlag(eAccountStatus.EMAIL_CONFIRM) && !d.status.HasFlag(eAccountStatus.BANNED) && !d.status.HasFlag(eAccountStatus.DISABLED)).ToList();
            return View();
        }

        public IActionResult findDealer()
        {
            ViewBag.Dealers = _SQL.dealers.Where(d=>d.status.HasFlag(eAccountStatus.EMAIL_CONFIRM) && !d.status.HasFlag(eAccountStatus.BANNED) && !d.status.HasFlag(eAccountStatus.DISABLED)).ToList();
            return View();
        }

        public IActionResult dealerPublicProfile(int dealerId)
        {
            ViewBag.Dealer = _SQL.dealers.SingleOrDefault(d => d.id == dealerId);
            List<Review> reviews = _SQL.reviews.Where(r => r.target_type == eVehicleTargetTypes.DEALER && r.target_id == dealerId).ToList();
            reviews.ForEach(r => {
                r.LoadReviewWriter();
            });
            ViewBag.Reviews = reviews;
            ViewBag.usersReview = _SQL.reviews.SingleOrDefault(r => r.target_id == dealerId && r.target_type == eVehicleTargetTypes.DEALER && r.source_type == loginType && r.source_id == loginId);
            
            return View();
        }
        
        public IActionResult servicePublicProfile(int serviceId)
        {
            ViewBag.Service = _SQL.services.SingleOrDefault(s=>s.id == serviceId);
			List <Review> reviews = _SQL.reviews.Where(r => r.target_type == eVehicleTargetTypes.SERVICE && r.target_id == serviceId).ToList();
			reviews.ForEach(r => {
				r.LoadReviewWriter();
			});
			ViewBag.Reviews = reviews;
			ViewBag.UsersReview = _SQL.reviews.SingleOrDefault(r => r.target_id == serviceId && r.target_type == eVehicleTargetTypes.SERVICE && r.source_type == loginType && r.source_id == loginId);

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

        [HttpPost]
        public async Task<IActionResult> createVehicleSaleAdvert(string vehicleId, int? vehiclePrice, string userPhone, string userMail, DateTime? saleStartDate, string saleDescription, bool? saleAvailable) //Nem szép, de legalább működik
        {
            if (string.IsNullOrEmpty(vehicleId) || vehiclePrice == null || vehiclePrice < 1 || string.IsNullOrEmpty(userPhone) || string.IsNullOrEmpty(userMail) || string.IsNullOrEmpty(saleDescription) || saleStartDate == null || saleAvailable == null)
            {
                _Notification.AddErrorToastMessage("Minden adatot ki kell tölteni!");
                return BadRequest("Minden adatot ki kell tölteni!");
            }
            Vehicle v = _SQL.vehicles.SingleOrDefault(vh => vh.chassis_number == vehicleId);
            if (v == null)
            {
                _Notification.AddErrorToastMessage("A keresett jármű nem található!");
                return NotFound("A keresett jármű nem található!");
            }
            if (!_SQL.vehiclePermissions.Any(vp => vp.vehicle_id == vehicleId && vp.target_type == loginType && vp.target_id == loginId && vp.permission == eVehiclePermissions.OWNER))
            {
                _Notification.AddErrorToastMessage("Nincs jogosultsága ehhez a művelethez!");
                return Forbid("Nincs jogosultsága ehhez a művelethez!");
            }
            if (_SQL.vehicleSales.Any(s => s.vehicle_id == vehicleId))
            {
                _Notification.AddErrorToastMessage("A jármű már eladásra lett kínálva!");
                return Conflict("A jármű már eladásra lett kínálva!");
            }
            SaleVehicle sv = new SaleVehicle()
            {
                transaction_id = Guid.NewGuid(),
                vehicle_id = vehicleId,
                announcement_date = (DateTime)saleStartDate,
                description = saleDescription,
                email = userMail,
                phone = userPhone,
                vehicle_cost = (int)vehiclePrice,
                active = true
            };
            _SQL.vehicleSales.Add(sv);
            _SQL.SaveChanges();

            var filePath = _Environment.WebRootPath + "/Images/SaleImages/";
            Directory.CreateDirectory(filePath + "/" + sv.transaction_id.ToString() + "/");
            filePath += "/" + sv.transaction_id.ToString() + "/";
            int i = 0;
            foreach (IFormFile formFile in Request.Form.Files)
            {
                if (formFile.Length > 0 && (Path.GetExtension(formFile.FileName) == ".png" || Path.GetExtension(formFile.FileName) == ".jpg" || Path.GetExtension(formFile.FileName) == ".jpeg"))
                {
                    using (var stream = System.IO.File.Create(filePath + +i + Path.GetExtension(formFile.FileName)))
                    {
                        await formFile.CopyToAsync(stream);
                        i++;
                    }
                }
                
            }

            _Notification.AddSuccessToastMessage("A jármű sikeresen eladásra lett kínálva!");
            return Ok("A jármű sikeresen eladásra kínálva!");
        }

        [HttpPost]
        public async Task<IActionResult> deleteSaleImage(string image, Guid transactionId)
        {
            SaleVehicle sv = _SQL.vehicleSales.SingleOrDefault(s => s.transaction_id == transactionId);
            if(sv == null) {
                _Notification.AddErrorToastMessage("A keresett tranzakció nem található!");
                return BadRequest("A keresett tranzakció nem található!");
            }
            if (string.IsNullOrWhiteSpace(image)) {
                _Notification.AddErrorToastMessage("Hibás kép!");
                return BadRequest("Hibás kép!");
            }

            if(!_SQL.vehiclePermissions.Any(vp=>vp.vehicle_id == sv.vehicle_id && vp.permission == eVehiclePermissions.OWNER && vp.target_id == loginId && vp.target_type == loginType)) {
                _Notification.AddErrorToastMessage("A felhasználónak nincs jogosultsága a művelethez!");
                return BadRequest("A felhasználónak nincs jogosultsága a művelethez!");
            }

            if(!System.IO.File.Exists(_Environment.WebRootPath + image)){
                _Notification.AddErrorToastMessage("A keresett kép nem található!");
                return NotFound("A keresett kép nem található!");
            }
            else {
                System.IO.File.Delete(_Environment.WebRootPath + image);

                List<string> files = Directory.GetFiles(_Environment.WebRootPath + "/Images/SaleImages/"+transactionId).ToList();
                int index = 0;
                if(files.Count > 0) { 
                    foreach(string file in files) {
                        string extension = Path.GetExtension("/Images/SaleImages/" + transactionId + "/" + file);
                        System.IO.File.Move(file, _Environment.WebRootPath + "/Images/SaleImages/" + transactionId + "/" + index + extension);
                        index++;
                    }
                }

                _Notification.AddSuccessToastMessage("Kép sikeresen törölve!");
                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> saveImagePosition(string image, Guid transactionId, int position)
        {
            SaleVehicle sv = _SQL.vehicleSales.SingleOrDefault(s => s.transaction_id == transactionId);
            if (sv == null)
            {
                _Notification.AddErrorToastMessage("A keresett tranzakció nem található!");
                return BadRequest("A keresett tranzakció nem található!");
            }
            if (string.IsNullOrWhiteSpace(image))
            {
                _Notification.AddErrorToastMessage("Hibás kép!");
                return BadRequest("Hibás kép!");
            }

            if (!_SQL.vehiclePermissions.Any(vp => vp.vehicle_id == sv.vehicle_id && vp.permission == eVehiclePermissions.OWNER && vp.target_id == loginId && vp.target_type == loginType))
            {
                _Notification.AddErrorToastMessage("A felhasználónak nincs jogosultsága a művelethez!");
                return BadRequest("A felhasználónak nincs jogosultsága a művelethez!");
            }

            if (!System.IO.File.Exists(_Environment.WebRootPath + image))
            {
                _Notification.AddErrorToastMessage("A keresett kép nem található!");
                return NotFound("A keresett kép nem található!");
            }
            else
            {
                var files = Directory.GetFiles(_Environment.WebRootPath + "/Images/SaleImages/" + transactionId + "/");

                if (files.Any(tmp=>Path.GetFileNameWithoutExtension(tmp) == position.ToString()))
                {
                    string oldextension = Path.GetExtension(files.Single(tmp => Path.GetFileNameWithoutExtension(tmp) == position.ToString()));
                    System.IO.File.Move(files.Single(tmp => Path.GetFileNameWithoutExtension(tmp) == position.ToString()), _Environment.WebRootPath + "/Images/SaleImages/"+ transactionId + "tmp.file");

                    string oldPosition = Path.GetFileNameWithoutExtension(_Environment.WebRootPath + image);
                    System.IO.File.Move(_Environment.WebRootPath + image, _Environment.WebRootPath + "/Images/SaleImages/" + transactionId+ "/" + position + Path.GetExtension(_Environment.WebRootPath + image));

                    System.IO.File.Move(_Environment.WebRootPath + "/Images/SaleImages/" + transactionId + "tmp.file", _Environment.WebRootPath + "/Images/SaleImages/" + transactionId+ "/" +oldPosition +oldextension);

                }
                else {
                    System.IO.File.Move(_Environment.WebRootPath + image, _Environment.WebRootPath + "/Images/SaleImages/" + transactionId + "/" + position + Path.GetExtension(_Environment.WebRootPath + image));
                }
                _Notification.AddSuccessToastMessage("Kép sikeresen törölve!");
                return Ok();
            }
        }

        [HttpPost]
        public async Task<IActionResult> updateVehicleSale(Guid transactionId, int? vehiclePrice, string userPhone, string userMail, DateTime? saleStartDate, string saleDescription, bool? saleAvailable)
        {
            SaleVehicle sv = _SQL.vehicleSales.SingleOrDefault(s => s.transaction_id == transactionId);
            if (sv == null)
            {
                _Notification.AddErrorToastMessage("A keresett tranzakció nem található!");
                return BadRequest("A keresett tranzakció nem található!");
            }

            if (!_SQL.vehiclePermissions.Any(vp => vp.vehicle_id == sv.vehicle_id && vp.permission == eVehiclePermissions.OWNER && vp.target_id == loginId && vp.target_type == loginType))
            {
                _Notification.AddErrorToastMessage("A felhasználónak nincs jogosultsága a művelethez!");
                return BadRequest("A felhasználónak nincs jogosultsága a művelethez!");
            }

            if(vehiclePrice != null && sv.vehicle_cost != vehiclePrice) {
                sv.vehicle_cost = (int)vehiclePrice;
            }
            if(!string.IsNullOrWhiteSpace(userPhone) && userPhone != sv.phone) { 
                sv.phone = userPhone;
            }
            if(!string.IsNullOrWhiteSpace(userMail) && userMail != sv.email) { 
                sv.email = userMail;
            }

            if(saleStartDate != null && saleStartDate != sv.announcement_date) {
                sv.announcement_date = (DateTime)saleStartDate;
            }
            if (!string.IsNullOrWhiteSpace(saleDescription) && saleDescription != sv.description) { 
                sv.description = saleDescription;
            }
            if(saleAvailable != null && saleAvailable != sv.active) {
                sv.active = (bool)saleAvailable;
            }

            _SQL.vehicleSales.Update(sv);
            _SQL.SaveChangesAsync();

            var filePath = _Environment.WebRootPath + "/Images/SaleImages/";
            filePath += "/" + transactionId.ToString() + "/";
            if (Request.Form.Files.Count > 0)
            {
                int i = Directory.GetFiles(_Environment.WebRootPath + "/Images/SaleImages/" + transactionId + "/").Length;
                foreach (IFormFile formFile in Request.Form.Files)
                {
                    if (formFile.Length > 0 && (Path.GetExtension(formFile.FileName) == ".png" || Path.GetExtension(formFile.FileName) == ".jpg" || Path.GetExtension(formFile.FileName) == ".jpeg"))
                    {
                        using (var stream = System.IO.File.Create(filePath + +i + Path.GetExtension(formFile.FileName)))
                        {
                            await formFile.CopyToAsync(stream);
                            i++;
                        }
                    }

                }
            }
            _Notification.AddSuccessToastMessage("A módosítások sikeresen érvényesítve lettek!");
            return Ok("A módosítások sikeresen érvényesítve lettek!");
        }

        [HttpPost]
        public async Task<IActionResult> removeSaleTransaction(Guid transactionId)
        {
            SaleVehicle sv = _SQL.vehicleSales.SingleOrDefault(s => s.transaction_id == transactionId);
            if (sv == null)
            {
                _Notification.AddErrorToastMessage("A keresett tranzakció nem található!");
                return BadRequest("A keresett tranzakció nem található!");
            }

            if (!_SQL.vehiclePermissions.Any(vp => vp.vehicle_id == sv.vehicle_id && vp.permission == eVehiclePermissions.OWNER && vp.target_id == loginId && vp.target_type == loginType))
            {
                _Notification.AddErrorToastMessage("A felhasználónak nincs jogosultsága a művelethez!");
                return BadRequest("A felhasználónak nincs jogosultsága a művelethez!");
            }

            _SQL.vehicleSales.Remove(sv);
            _SQL.SaveChanges();

            var filePath = _Environment.WebRootPath + "/Images/SaleImages/" + transactionId.ToString();

            Directory.Delete(filePath, true);

            _Notification.AddSuccessToastMessage("Sikeres tranzakció törlés!");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int rating, int target_id, eVehicleTargetTypes target_type, string review)
        {
            if(target_type != eVehicleTargetTypes.SERVICE && target_type != eVehicleTargetTypes.DEALER)
            {
                _Notification.AddErrorToastMessage("Hibás értékelés: célcsoport");
				return Redirect("/");
			}

            switch (target_type)
            {
                case eVehicleTargetTypes.DEALER:
                    if (!_SQL.dealers.Any(d => d.id == target_id)) {
                        _Notification.AddErrorToastMessage("Kereskedő nem található!");
						return Redirect("/");
					}
                break;

                case eVehicleTargetTypes.SERVICE:
					if (!_SQL.services.Any(s => s.id == target_id))
					{
						_Notification.AddErrorToastMessage("Szerviz nem található!");
						return Redirect("/");
					}
				break;
            }

			if (rating > 5 || rating < 1)
			{
				_Notification.AddErrorToastMessage("Hibás értékelés: csillag");
				switch (target_type)
				{
					case eVehicleTargetTypes.DEALER:
						return dealerPublicProfile(target_id);
					case eVehicleTargetTypes.SERVICE:
						return servicePublicProfile(target_id);
					default:
						return Redirect("/");
				}
			}

			if (_SQL.reviews.Any(r=>r.target_id == target_id && r.target_type == target_type && r.source_type == loginType && r.source_id == loginId))//Módosítás
            {
                Review rev = _SQL.reviews.Single(r => r.target_id == target_id && r.target_type == target_type && r.source_type == loginType && r.source_id == loginId);
				rev.edited = true;
				rev.description = review;
				rev.rating = rating;

                _SQL.reviews.Update(rev);
                await _SQL.SaveChangesAsync();
				_Notification.AddSuccessToastMessage("Sikeres véleménymódosítás");
			}
            else //Hozzáad
            {
				_SQL.reviews.Add(new Review() { date = DateTime.Now, description = review, edited = false, id = Guid.NewGuid(), rating = rating, source_id = loginId, source_type = loginType, target_id = target_id, target_type = target_type });
                await _SQL.SaveChangesAsync();
				_Notification.AddSuccessToastMessage("Sikeres véleményírás");
			}
            switch (target_type)
            {
                case eVehicleTargetTypes.DEALER:
					return Redirect("dealerPublicProfile?dealerId=" + target_id);
				case eVehicleTargetTypes.SERVICE:
                    return Redirect("servicePublicProfile?serviceId=" + target_id);
                default:
                    return Redirect("/");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveReview(Guid reviewId)
        {
            if(!_SQL.reviews.Any(r=>r.id == reviewId))
            {
				_Notification.AddErrorToastMessage("A vélemény nem található!");
                return Redirect("/");
			}

            Review rev = _SQL.reviews.Single(r => r.id == reviewId);

            eVehicleTargetTypes redirectType = rev.target_type;
            int redirectId = rev.target_id;

            if(rev.source_id == loginId && rev.source_type == loginType)
            {
                _SQL.reviews.Remove(rev);
                await _SQL.SaveChangesAsync();
                _Notification.AddSuccessToastMessage("Vélemény sikeresen törölve!");
            }
            else
            {
				_Notification.AddErrorToastMessage("Nincs jogosultság törölni ezt a véleményt!");
			}

			switch (redirectType)
			{
				case eVehicleTargetTypes.DEALER:
					return Redirect("dealerPublicProfile?dealerId=" + redirectId);
				case eVehicleTargetTypes.SERVICE:
					return Redirect("servicePublicProfile?serviceId=" + redirectId);
				default:
					return Redirect("/");
			}

		}
	#endregion
	}
}

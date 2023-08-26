using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Diagnostics;

namespace AutoPortal.Controllers
{
    public class HomeController : BaseController
    {
        /*private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }*/

        public HomeController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult MyProfile()
        {
            switch (loginType)
            {
                case eVehicleTargetTypes.DEALER:
                    ViewBag.loginType = "Kereskedő";
                    ViewBag.pageCategory = 2;
                    ViewBag.id = user.id;
                    ViewBag.email = user.email;
                    ViewBag.phone = user.phone;
                    ViewBag.name = user.name;
                    ViewBag.description = user.description;
                    ViewBag.statuses = EnumHelper.GetStatusStringList(user.status);
                    ViewBag.country = user.country;
                    ViewBag.city = user.city;
                    ViewBag.address = user.address;
                    ViewBag.website = user.website;
                    break;
                case eVehicleTargetTypes.SERVICE:
                    ViewBag.loginType = "Szerviz";
                    ViewBag.pageCategory = 1;
                    ViewBag.id = user.id;
                    ViewBag.email = user.email;
                    ViewBag.phone = user.phone;
                    ViewBag.name = user.name;
                    ViewBag.description = user.description;
                    ViewBag.statuses = EnumHelper.GetStatusStringList(user.status);
                    ViewBag.country = user.country;
                    ViewBag.city = user.city;
                    ViewBag.address = user.address;
                    ViewBag.website = user.website;
                    break;
                case eVehicleTargetTypes.USER:
                    ViewBag.loginType = "Felhasználó";
                    ViewBag.pageCategory = 0;
                    ViewBag.id = user.id;
                    ViewBag.email = user.email;
                    ViewBag.name = user.name;
                    ViewBag.register = user.register_date;
                    ViewBag.statuses = EnumHelper.GetStatusStringList(user.status);
                    ViewBag.roles = user.GetRoles();
                    break;
            }
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> saveUserChanges(string email, string name)
        {
            if (user != null)
            {
                if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
                {
                    _Notification.AddErrorToastMessage("Minden mezőt ki kell tölteni!");
                    return BadRequest("Bad data!");
                }
                bool change = false;
                if(user.email != email) {
                    if(_SQL.users.Any(u=>u.email == email) || _SQL.services.Any(s=>s.email == email) || _SQL.dealers.Any(d=>d.email == email) || _SQL.factories.Any(f=>f.email == email))
                    {
                        _Notification.AddInfoToastMessage("Sikertelen módosítás! Az E-mail cím már használatban van.");
                        return BadRequest("Az email cím már használatban van");
                    }
                    else {
                        change = true;
                        user.email = email;
                    }
                }
                if(user.name != name) { 
                    change = true;
                    user.name = name;
                }

                if (change) {
                    _SQL.users.Update(user);
                    _SQL.SaveChanges();
                    _Notification.AddSuccessToastMessage("Sikeres módosítás!");
                    return Ok("Sikeres módosítás!");
                }
                else {
                    _Notification.AddInfoToastMessage("Nem történt módosítás!");
                    return Ok("Nem történt módosítás!");
                }
                
            }
            else
            {
                _Notification.AddErrorToastMessage("Auth hiba!");
                return BadRequest("Auth hiba!");
            }
        }

        [HttpPost]
        public async Task<IActionResult> changeUserPassword(string oldPwd, string newPwd, string newPwdRe)
        {
            if(user == null || loginType != eVehicleTargetTypes.USER) {
                _Notification.AddErrorToastMessage("Auth hiba!");
                return BadRequest("Auth hiba!");
            }
            if(!PasswordManager.AreEqual(oldPwd, user.password)) {
                _Notification.AddInfoToastMessage("A régi jelszó nem megfelelő!");
                return BadRequest("A régi jelszó nem megfelelő!");
            }
            if(string.IsNullOrEmpty(newPwd) || newPwd.Length < 6) {
                _Notification.AddInfoToastMessage("Az új jelszónak legalább 6 karaktert kell tartalmaznia!");
                return BadRequest("Az új jelszónak legalább 6 karaktert kell tartalmaznia!");
            }
            if(newPwd != newPwdRe) {
                _Notification.AddInfoToastMessage("Az új jelszavak nem egyeznek!");
                return BadRequest("Az új jelszavak nem egyeznek.");
            }

            user.password = PasswordManager.GenerateHash(newPwd);

            _SQL.users.Update(user);
            _SQL.SaveChanges();
            _Notification.AddSuccessToastMessage("Sikeres jelszó módosítás!");
            return Ok("Sikeres jelszó módosítás!");
        }

        [HttpPost]
        public async Task<IActionResult> saveCompanyChanges([FromBody] UpdateCompanyDataModel m)
        {
            if(user == null)
            {
                _Notification.AddErrorToastMessage("Auth hiba!");
                return BadRequest("Auth hiba!");
            }
            if(loginType != eVehicleTargetTypes.SERVICE && loginType != eVehicleTargetTypes.DEALER) {
                _Notification.AddErrorToastMessage("Hibás fiók!");
                return BadRequest("Hibás fiók!");
            }
            if (!ModelState.IsValid)
            {
                _Notification.AddErrorToastMessage("Hiányos adatok!");
                return BadRequest("Hiányos adatok!");
            }
            
            if(user.email != m.email) {
                if (_SQL.users.Any(u => u.email == m.email) || _SQL.services.Any(s => s.email == m.email) || _SQL.dealers.Any(d => d.email == m.email) || _SQL.factories.Any(f => f.email == m.email))
                {
                    _Notification.AddInfoToastMessage("Sikertelen módosítás! Az E-mail cím már használatban van.");
                    return BadRequest("Az email cím már használatban van");
                }
                else
                {
                    user.email = m.email;
                }
            }
            if(user.phone != m.phone) { 
                user.phone = m.phone;
            }
            if(user.name != m.name) {
                user.name = m.name;
            }
            if(user.description != m.description) { 
                user.description = m.description;
            }
            if(user.country != m.country) {
                user.county = m.country;
            }
            if(user.city != m.city) { 
                user.city = m.city;
            }
            if(user.address != m.address) { 
                user.address = m.address;
            }
            if(user.website != m.website) { 
                user.website = m.website;
            }

            if(loginType == eVehicleTargetTypes.SERVICE) {
                _SQL.services.Update(user);
            }
            else if(loginType == eVehicleTargetTypes.DEALER) {
                _SQL.dealers.Update(user);
            }
            else {
                _Notification.AddErrorToastMessage("Hibás fiók!");
                return BadRequest("Hibás fiók!");
            }
            _SQL.SaveChanges();
            _Notification.AddSuccessToastMessage("Sikeres módosítás!");
            return Ok("Sikeres módosítás!");
        }

        [HttpPost]
        public async Task<IActionResult> changeCompanyPassword(string oldPwd, string newPwd, string newPwdRe)
        {
            if (user == null)
            {
                _Notification.AddErrorToastMessage("Auth hiba!");
                return BadRequest("Auth hiba!");
            }

            if (!PasswordManager.AreEqual(oldPwd, user.password))
            {
                _Notification.AddInfoToastMessage("A régi jelszó nem megfelelő!");
                return BadRequest("A régi jelszó nem megfelelő!");
            }
            if (string.IsNullOrEmpty(newPwd) || newPwd.Length < 6)
            {
                _Notification.AddInfoToastMessage("Az új jelszónak legalább 6 karaktert kell tartalmaznia!");
                return BadRequest("Az új jelszónak legalább 6 karaktert kell tartalmaznia!");
            }
            if (newPwd != newPwdRe)
            {
                _Notification.AddInfoToastMessage("Az új jelszavak nem egyeznek!");
                return BadRequest("Az új jelszavak nem egyeznek.");
            }

            user.password = PasswordManager.GenerateHash(newPwd);

            if (loginType == eVehicleTargetTypes.SERVICE)
            {
                _SQL.services.Update(user);
            }
            else if(loginType == eVehicleTargetTypes.DEALER)
            {
                _SQL.dealers.Update(user);
            }
            else
            {
                _Notification.AddErrorToastMessage("Hibás fiók!");
                return BadRequest("Hibás fiók!");
            }
            _SQL.SaveChanges();
            _Notification.AddSuccessToastMessage("Sikeres jelszó módosítás!");
            return Ok("Sikeres jelszó módosítás!");
        }
    }
}
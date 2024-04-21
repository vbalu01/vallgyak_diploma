using AutoPortal.Libs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NToastNotify;
using MySqlConnector;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using AutoPortal.Models.AppModels;

namespace AutoPortal.Controllers
{
    public class DEVController : BaseController
    {
        public DEVController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        { }
        public IActionResult CheckDbConnection()
        {
            dynamic returnObj = new System.Dynamic.ExpandoObject();

            returnObj.CanConnect = _SQL.Database.CanConnect();

            Dictionary<string, bool> tables = checkTables();
            returnObj.tables = tables;
            
            ViewBag.data = JsonConvert.SerializeObject(returnObj);

            return View("index");
        }

        public async Task<IActionResult> MailSend()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendDevMail(string to, string subject, string body)
        {
            if(string.IsNullOrEmpty(to) || string.IsNullOrEmpty(subject))
            {
                _Notification.AddErrorToastMessage("Levél küldés sikertelen: Adathiba!");
                return BadRequest("Levél küldés sikertelen: Adathiba!");
            }
            MailModel m = new MailModel()
            {
                to = to,
                body = body,
                subject = subject,
                from = "dev@AutoPortal.hu",
                isHtml = true,
            };
            bool res = await MailSender.SendMail(m);

            if (res) {
                _Notification.AddSuccessToastMessage("Email sikeresen elküldve!", new ToastrOptions() { Title = "Siker" });
                return Ok("Sikeres levél küldés!");
            }
            else {
                _Notification.AddErrorToastMessage("Levél küldés sikertelen!");
                return BadRequest("Levél küldés sikertelen!");
            }
                
        }

        private Dictionary<string, bool> checkTables()
        {
            Dictionary<string, bool> tables = new();
            try { _SQL.bodyTypes.Any(); tables.Add("BodyType", true);} catch { tables.Add("BodyType", false);}
            try { _SQL.driveTypes.Any(); tables.Add("DriveType", true);} catch { tables.Add("DriveType", false);}
            try { _SQL.fuelTypes.Any(); tables.Add("FuelType", true);} catch { tables.Add("FuelType", false);}
            try { _SQL.roles.Any(); tables.Add("Role", true);} catch { tables.Add("Role", false);}
            try { _SQL.transmissionTypes.Any(); tables.Add("TransmissionType", true);} catch { tables.Add("TransmissionType", false);}
            try { _SQL.users.Any(); tables.Add("User", true);} catch { tables.Add("User", false);}
            try { _SQL.userRoles.Any(); tables.Add("UserRole", true);} catch { tables.Add("UserRole", false);}
            try { _SQL.vehicles.Any(); tables.Add("Vehicle", true);} catch { tables.Add("Vehicle", false);}
            try { _SQL.vehicleCategories.Any(); tables.Add("VehicleCategory", true);} catch { tables.Add("VehicleCategory", false);}
            try { _SQL.vehicleMakes.Any(); tables.Add("VehicleMake", true);} catch { tables.Add("VehicleMake", false);}
            try { _SQL.vehicleModels.Any(); tables.Add("VehicleModel", true);} catch { tables.Add("VehicleModel", false);}
            try { _SQL.vehiclePermissions.Any(); tables.Add("VehiclePermission", true);} catch { tables.Add("VehiclePermission", false);}
            try { _SQL.otherCosts.Any(); tables.Add("OtherCost", true);} catch { tables.Add("OtherCost", false);}
            try { _SQL.crashEvents.Any(); tables.Add("CrashEvent", true);} catch { tables.Add("CrashEvent", false);}
            try { _SQL.vehicleOwnerChanges.Any(); tables.Add("VehicleOwnerChange", true);} catch { tables.Add("VehicleOwnerChange", false);}
            try { _SQL.refuels.Any(); tables.Add("Refuel", true);} catch { tables.Add("Refuel", false);}
            try { _SQL.serviceEvents.Any(); tables.Add("ServiceEvent", true);} catch { tables.Add("ServiceEvent", false);}
            try { _SQL.services.Any(); tables.Add("Service", true);} catch { tables.Add("Service", false);}
            try { _SQL.factories.Any(); tables.Add("Factory", true);} catch { tables.Add("Factory", false);}
            try { _SQL.dealers.Any(); tables.Add("Dealer", true);} catch { tables.Add("Dealer", false);}
            try { _SQL.reviews.Any(); tables.Add("Review", true);} catch { tables.Add("Review", false);}
            try { _SQL.mileageStands.Any(); tables.Add("MileageStand", true);} catch { tables.Add("MileageStand", false);}
            try { _SQL.vehicleSales.Any(); tables.Add("VehicleSale", true);} catch { tables.Add("VehicleSale", false);}
            try { _SQL.tokens.Any(); tables.Add("Token", true);} catch { tables.Add("Token", false);}

            return tables;
        }

        [HttpGet]
        public async Task<IActionResult> NotifySuccess(string msg)
        {
            _Notification.AddSuccessToastMessage(msg);
            return Redirect("/");
        }
        [HttpGet]
        public async Task<IActionResult> NotifyWarning(string msg)
        {
            _Notification.AddWarningToastMessage(msg);
            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> NotifyError(string msg)
        {
            _Notification.AddErrorToastMessage(msg);
            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> NotifyInfo(string msg)
        {
            _Notification.AddInfoToastMessage(msg);
            return Redirect("/");
        }
    }
}
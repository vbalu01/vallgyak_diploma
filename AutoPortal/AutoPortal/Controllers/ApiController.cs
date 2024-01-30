using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data.SqlTypes;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace AutoPortal.Controllers
{
    [Route("/api/factory")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApiFactoryController : Controller
    {
        private JsonResponse response;
        private int loginId;
        public ApiFactoryController() {
            response = new();
            response.Success = false;
        }
        public override void OnActionExecuting(ActionExecutingContext context) //Ez a metódus minden API hívás előtt lefut
        {
            this.loginId = Convert.ToInt32(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "factoryId").Value);
            base.OnActionExecuting(context);
        }

        [HttpGet("test")]
        public string test() {
            response.Message = "Success";
            response.Success = true;
            return response.ToString();
        }

        [HttpGet("getVehicles")]
        public async Task<IActionResult> getVehicles()
        {
            List<VehiclePermission> vehiclePermissions = new List<VehiclePermission>();
            List<Vehicle> vehicles = new List<Vehicle>();
            using(SQL mysql = new SQL())
            {
                vehiclePermissions = mysql.vehiclePermissions.Where(vp => vp.permission == eVehiclePermissions.OWNER && vp.target_type == eVehicleTargetTypes.FACTORY && vp.target_id == loginId).ToList();
                foreach(var item in vehiclePermissions)
                {
                    vehicles.Add(mysql.vehicles.Single(v => v.chassis_number == item.vehicle_id));
                }
            }
            response.Success = true;
            response.Message = JsonConvert.SerializeObject(vehicles);
            return Ok(response.ToString());
        }

        [HttpPost("changePassword")]
        public async Task<IActionResult> changePassword(string? newPwd)
        {
            if(newPwd != null && newPwd.Length < 7)
            {
                response.Message = "New password must be at least 8 char length.";
                return BadRequest(response.ToString());
            }
            using(SQL mysql = new SQL())
            {
                Factory f = mysql.factories.Single(f => f.id == loginId);
                response.Message = "Password changed successfully.";
                if (string.IsNullOrEmpty(newPwd))
                {
                    newPwd = PasswordManager.GenerateRandomPassword();
                    await MailSender.SendNewFactoryPwdMail(f.email, f.name, newPwd);
                    response.Message = "New password sent in mail.";
                }
                f.password = PasswordManager.GenerateHash(newPwd);
                mysql.factories.Update(f);
                await mysql.SaveChangesAsync();
                response.Success = true;
                return Ok(response.ToString());
            }
        }

        [HttpPost("changeName")]
        public async Task<IActionResult> changeName(string? newName)
        {
            if (string.IsNullOrEmpty(newName) || newName.Length < 4)
            {
                response.Message = "New name must be at least 4 characters.";
                return BadRequest(response.ToString());
            }
            using (SQL mysql = new SQL())
            {
                Factory f = mysql.factories.Single(f => f.id == loginId);
                response.Message = "Name changed successfully.";
                f.name = newName;
                mysql.factories.Update(f);
                await mysql.SaveChangesAsync();
                response.Success = true;
                return Ok(response.ToString());
            }
        }

        [HttpPost("changeMail")]
        public async Task<IActionResult> changeMail(string? newMail)
        {
            if (string.IsNullOrEmpty(newMail) || !newMail.Contains('.') || !newMail.Contains('@'))
            {
                response.Message = "New mail is not valid.";
                return BadRequest(response.ToString());
            }
            using (SQL mysql = new SQL())
            {
                Factory f = mysql.factories.Single(f => f.id == loginId);
                if(f.email == newMail)
                {
                    response.Message = "Old and new mails are equal.";
                    return BadRequest(response.ToString());
                }
                if(mysql.factories.Any(f=>f.email == newMail) || mysql.users.Any(u=>u.email == newMail) || mysql.services.Any(s=>s.email == newMail) || mysql.dealers.Any(d=>d.email == newMail))
                {
                    response.Message = "New mail already in use.";
                    return BadRequest(response.ToString());
                }

                f.email = newMail;
                mysql.factories.Update(f);
                await mysql.SaveChangesAsync();
                response.Message = "Email changed successfully.";
                response.Success = true;
                return Ok(response.ToString());
            }
        }

        [HttpPost("addBrandNewVehicle")]
        public async Task<IActionResult> addBrandNewVehicle([FromBody] AddBrandNewCarModel m) { 
            if(ModelState.IsValid) {
                using(SQL mysql = new SQL()){
                    if(mysql.vehicles.Any(v=>v.chassis_number == m.chassis_number))
                    {
                        response.Message = "Chassis number already exists.";
                        return Conflict(response.ToString());
                    }
                }
                Vehicle v = new Vehicle(m);
                using(SQL mysql = new SQL())
                {
                    mysql.vehicles.Add(v);
                    await mysql.SaveChangesAsync();

                    if (mysql.users.Any(u => u.email == m.email) || mysql.factories.Any(u => u.email == m.email) || mysql.dealers.Any(u => u.email == m.email) || mysql.services.Any(u => u.email == m.email))
                    {
                        eVehicleTargetTypes targetType = eVehicleTargetTypes.FACTORY;
                        int targetId = loginId;
                        if (mysql.users.Any(u => u.email == m.email))
                        {
                            targetType = eVehicleTargetTypes.USER;
                            targetId = mysql.users.Single(u => u.email == m.email).id;
                        }
                        else if (mysql.factories.Any(u => u.email == m.email))
                        {
                            targetType = eVehicleTargetTypes.FACTORY;
                            targetId = mysql.factories.Single(f => f.email == m.email).id;
                        }
                        else if (mysql.dealers.Any(u => u.email == m.email))
                        {
                            targetType = eVehicleTargetTypes.DEALER;
                            targetId = mysql.dealers.Single(d => d.email == m.email).id;
                        }
                        else if (mysql.services.Any(u => u.email == m.email))
                        {
                            targetType = eVehicleTargetTypes.SERVICE;
                            targetId = mysql.services.Single(s => s.email == m.email).id;
                        }

                        mysql.vehiclePermissions.Add(new VehiclePermission()
                        {
                            permission = eVehiclePermissions.OWNER,
                            target_id = targetId,
                            target_type = targetType,
                            vehicle_id = v.chassis_number
                        });
                        await mysql.SaveChangesAsync();
                    }
                    response.Success= true;
                    response.Message = v.chassis_number;

                    return Ok(response.ToString());
                }
            }
            else {
                response.Success = false;
                response.Message = "DataError";
                return BadRequest(response);
            }
        }

        [HttpPost("changeVehicleOwner")]
        public async Task<IActionResult> changeVehicleOwner(string vehicleId, string newOwnerMail, bool keepPermissions = false)
        {
            using(SQL mysql = new SQL())
            {
                if(mysql.vehiclePermissions.Any(vp=>vp.vehicle_id == vehicleId && vp.target_type == eVehicleTargetTypes.FACTORY && vp.target_id == loginId))
                {
                    if(!(mysql.users.Any(u=>u.email == newOwnerMail) || mysql.services.Any(s=>s.email == newOwnerMail) || mysql.dealers.Any(d=>d.email == newOwnerMail) || mysql.factories.Any(f=>f.email == newOwnerMail)))
                    {
                        response.Message = "User not found with address.";
                        return BadRequest(response.ToString());
                    }

                    if(!keepPermissions)
                    {
                        mysql.vehiclePermissions.RemoveRange(mysql.vehiclePermissions.Where(vp=>vp.vehicle_id == vehicleId));
                    }

                    if(mysql.users.Any(u=>u.email == newOwnerMail))
                        mysql.vehiclePermissions.Add(new VehiclePermission() { vehicle_id = vehicleId, permission = eVehiclePermissions.OWNER, target_type = eVehicleTargetTypes.USER, target_id = mysql.users.Single(u => u.email == newOwnerMail).id });
                    else if(mysql.factories.Any(f=>f.email == newOwnerMail))
                        mysql.vehiclePermissions.Add(new VehiclePermission() { vehicle_id = vehicleId, permission = eVehiclePermissions.OWNER, target_type = eVehicleTargetTypes.FACTORY, target_id = mysql.factories.Single(f => f.email == newOwnerMail).id });
                    else if(mysql.services.Any(s=>s.email == newOwnerMail))
                        mysql.vehiclePermissions.Add(new VehiclePermission() { vehicle_id = vehicleId, permission = eVehiclePermissions.OWNER, target_type = eVehicleTargetTypes.SERVICE, target_id = mysql.services.Single(s => s.email == newOwnerMail).id });
                    else if(mysql.dealers.Any(d=>d.email == newOwnerMail))
                        mysql.vehiclePermissions.Add(new VehiclePermission() { vehicle_id = vehicleId, permission = eVehiclePermissions.OWNER, target_type = eVehicleTargetTypes.DEALER, target_id = mysql.dealers.Single(d => d.email == newOwnerMail).id });

                    await mysql.SaveChangesAsync();
                    response.Success = true;
                    response.Message = "Owner succesfully changed.";
                    return Ok(response.ToString());
                }
                else
                {
                    response.Message = "Vehicle not found or permission not granted to do this action.";
                    return BadRequest(response.ToString());
                }
            }
        }
    }

    [Route("/api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class ApiAuthController : ControllerBase
    {
        private JsonResponse response;

        public ApiAuthController()
        {
            response = new();
            response.Success = false;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            using (SQL mysql = new SQL())
            {
                if(mysql.factories.Any(f=>f.email == email))
                {
                    Factory f = mysql.factories.Single(f => f.email == email);
                    if (PasswordManager.AreEqual(password, f.password))
                    {
                        if (f.status.HasFlag(eAccountStatus.BANNED))
                        {
                            response.Message = "Factory banned by an administrator";
                            return StatusCode(403, response.ToString());
                        }
                        response.Message = _GenerateToken(f);
                        response.Success = true;
                        return Ok(response.ToString());
                    }
                    else
                    {
                        response.Message = "Bad username or password";
                        return BadRequest(response.ToString());
                    }
                }
                else
                {
                    response.Message = "Bad username or password";
                    return BadRequest(response.ToString());
                }
            }
        }

        [HttpGet("test")]
        public string test()
        {
            response.Message = "Success";
            response.Success = true;
            return response.ToString();
        }

        private string _GenerateToken(Factory f)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Startup.TokenKey);
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, f.email),
                new Claim(ClaimTypes.Name, f.name),
                new Claim("factoryId", f.id.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

}

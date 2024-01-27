using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoPortal.Controllers
{
    [Route("/api/factory")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApiFactoryController : ControllerBase
    {
        private JsonResponse response;
        private int loginId;
        public ApiFactoryController() {
            response = new();
            this.loginId = Convert.ToInt32(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "factoryId").Value);
        }

        [HttpGet("test")]
        public string test() {
            response.Message = "Szia";
            return JsonConvert.SerializeObject(response);
        }

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
            return Ok(response);
        }

        [HttpPost("addBrandNewVehicle")]
        public async Task<IActionResult> addBrandNewVehicle([FromBody] AddBrandNewCarModel m) { 
            if(ModelState.IsValid) {
                Vehicle v = new Vehicle(m);
                using(SQL mysql = new SQL())
                {
                    mysql.vehicles.Add(v);
                    await mysql.SaveChangesAsync();
                    mysql.vehiclePermissions.Add(new VehiclePermission() {
                        permission = eVehiclePermissions.OWNER, 
                        target_id = m.ownerId,
                        target_type = m.vehicleTargetTypes,
                        vehicle_id = v.chassis_number
                    });
                    mysql.vehicleOwnerChanges.Add(new VehicleOwnerChange() { 
                        id = new Guid(),
                        new_owner = m.ownerId,
                        owner_type = m.vehicleTargetTypes,
                        owner_change_date = DateTime.Now,
                        vehicle_id = v.chassis_number
                    });
                    response.Success= true;
                    response.Message = v.chassis_number;
                    await mysql.SaveChangesAsync();
                    return Ok(response);
                }
            }
            else {
                response.Success = false;
                response.Message = "DataError";
                return BadRequest(response);
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
                            return Forbid(JsonConvert.SerializeObject(response));
                        }
                        response.Message = _GenerateToken(f);
                        response.Success = true;
                        return Ok(JsonConvert.SerializeObject(response));
                    }
                    else
                    {
                        response.Message = "Bad username or password";
                        return BadRequest(JsonConvert.SerializeObject(response));
                    }
                }
                else
                {
                    response.Message = "Bad username or password";
                    return BadRequest(JsonConvert.SerializeObject(response));
                }
            }
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

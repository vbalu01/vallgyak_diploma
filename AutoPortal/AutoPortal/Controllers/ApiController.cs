using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
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
    [Authorize]
    public class ApiFactoryController : ControllerBase
    {
        private JsonResponse response;

        public ApiFactoryController() {
            response = new();
        }

        [HttpGet("test")]
        public string test() {
            response.Message = "Szia";
            return JsonConvert.SerializeObject(response);
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

        [HttpPost("loginTest")]
        public async Task<IActionResult> LoginTest(string email, string name)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.TokenKey));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var tokenOptions = new JwtSecurityToken(
                claims: new List<Claim>() { new Claim(ClaimTypes.Name, name ?? string.Empty) },
                expires: DateTime.Now.AddDays(1),
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            return Ok(new { Token = tokenString });
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

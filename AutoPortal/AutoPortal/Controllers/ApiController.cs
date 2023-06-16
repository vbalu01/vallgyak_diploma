using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AutoPortal.Controllers
{
    [Route("/api")]
    [ApiController]
    //[Authorize("Factory")]
    public class ApiController : ControllerBase
    {
        private JsonResponse response;

        public ApiController() {
            response = new();
        }

        [HttpGet("test")]
        public string test() {
            response.Message = "Szia";
            return JsonConvert.SerializeObject(response);
        }

        [HttpPost("addBrandNewVehicle")]
        public IActionResult addBrandNewVehicle([FromBody] AddBrandNewCarModel m) { 
            if(ModelState.IsValid) {
                Vehicle v = new Vehicle(m);
                using(SQL mysql = new SQL())
                {
                    mysql.vehicles.Add(v);
                    mysql.SaveChanges();
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
}

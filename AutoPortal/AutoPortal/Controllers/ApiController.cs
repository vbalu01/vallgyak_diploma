using AutoPortal.Models;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AutoPortal.Controllers
{
    [Route("/api")]
    [ApiController]
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
        public string addBrandNewVehicle([FromBody] AddBrandNewCarModel m) { 
            if(ModelState.IsValid) { 
                
            }

            return "";
        }
    }
}

using AutoPortal.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AutoPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ApiTestController : ControllerBase
    {
        private JsonResponse response;

        public ApiTestController()
        {
            response = new();
        }
        [HttpGet("test")]
        public string test()
        {
            response.Message = "Szia";
            return JsonConvert.SerializeObject(response);
        }
    }
}

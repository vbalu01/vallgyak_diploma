using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;

namespace AutoPortal.Models.ResponseModels
{
    public class UserVehicle
    {
        public Vehicle v { get; set; }
        public eVehiclePermissions p { get; set; }
    }
}

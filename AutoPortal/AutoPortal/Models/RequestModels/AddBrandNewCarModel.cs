using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AddBrandNewCarModel : AddUserCarModel
    {
        [Required]
        public int ownerId { get; set; }
        [Required]
        public eVehicleTargetTypes vehicleTargetTypes { get; set; }
    }
}

using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehiclepermissions")]
    public partial class VehiclePermission
    {
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public int target_id { get; set; }
        [Required]
        public eVehicleTargetTypes target_type { get; set; }
        [Required]
        public eVehiclePermissions permission { get; set; }
    }
}

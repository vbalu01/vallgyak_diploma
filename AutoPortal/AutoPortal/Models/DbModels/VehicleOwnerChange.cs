using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehicleownerchanges")]
    public class VehicleOwnerChange
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public eVehicleTargetTypes owner_type { get; set; }
        [Required]
        public int new_owner { get; set; }
        [Required]
        public DateTime owner_change_date { get; set; }
    }
}

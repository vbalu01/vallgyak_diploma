using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehiclemodels")]
    public class VehicleModel
    {
        [Required]
        public string model { get; set; }
        [Required]
        public string make { get; set; }
        [Required]
        public int category { get; set; }
    }
}

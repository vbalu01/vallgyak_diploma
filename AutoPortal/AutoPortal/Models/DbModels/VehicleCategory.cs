using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehiclecategories")]
    public class VehicleCategory
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string category { get; set; }
    }
}

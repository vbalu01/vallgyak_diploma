using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehiclemakes")]
    public class VehicleMake
    {
        [Key]
        [Required]
        public string make { get; set; }
    }
}

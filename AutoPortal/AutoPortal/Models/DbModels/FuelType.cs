using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("fueltypes")]
    public class FuelType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string fuel { get; set; }
    }
}

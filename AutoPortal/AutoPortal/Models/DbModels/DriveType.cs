using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("drivetypes")]
    public class DriveType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string drive { get; set; }
    }
}

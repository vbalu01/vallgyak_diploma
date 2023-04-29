using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("transmissiontypes")]
    public class TransmissionType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string transmission { get; set; }
    }
}
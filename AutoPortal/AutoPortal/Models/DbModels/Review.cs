using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("reviews")]
    public class Review
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public eVehicleTargetTypes source_type { get; set; }
        [Required]
        public int source_id { get; set; }
        [Required]
        public eVehicleTargetTypes target_type { get; set; }
        [Required]
        public int target_id { get; set;}
        [Required]
        public int rating { get; set;}
        [Required]
        public string description { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public bool edited { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("crashevents")]
    public class CrashEvent
    {
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public int mileage { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public int damageCost { get; set; }
    }
}

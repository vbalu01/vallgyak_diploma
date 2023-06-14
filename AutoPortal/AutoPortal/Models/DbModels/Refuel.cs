using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("refuels")]
    public class Refuel
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public int refuel_cost { get; set; }
        [Required]
        public int traveled_distance { get; set; }
        [Required]
        public int amount_of_fuel { get; set; }
        [Required]
        public bool premium_fuel { get; set; }
        [Required]
        public DateTime fueling_date { get; set; }
    }
}

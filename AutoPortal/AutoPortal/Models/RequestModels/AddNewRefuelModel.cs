using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AddNewRefuelModel
    {
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

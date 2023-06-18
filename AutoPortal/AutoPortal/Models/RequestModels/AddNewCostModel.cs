using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AddNewCostModel
    {
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public int cost { get; set; }
        [Required]
        public DateTime date { get; set; }
    }
}

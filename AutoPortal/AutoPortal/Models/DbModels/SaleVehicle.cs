using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("salevehicles")]
    public class SaleVehicle
    {
        [Key]
        [Required]
        public Guid transaction_id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public int vehicle_cost { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public DateTime announcement_date { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public bool active { get; set; }
        [Required]
        public int dealerId { get; set; } = 0;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("othercosts")]
    public class OtherCost
    {
        [Key]
        [Required]
        public Guid id { get; set; }
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

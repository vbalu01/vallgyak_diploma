using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("tokens")]
    public class Token
    {
        [Key]
        [Required]
        public string token { get; set; }
        [Required]
        public eTokenType token_type { get; set; }
        public eVehicleTargetTypes target_type { get; set; }
        public int target_id { get; set; }
        public DateTime expire { get; set; } = DateTime.Now.AddHours(24);
        public int remain { get; set; } = 1;
        public bool available { get; set; } = true;
    }
}

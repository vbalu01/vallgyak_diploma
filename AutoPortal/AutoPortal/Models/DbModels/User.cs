using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("users")]
    public class User
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string password { get; set; }
        public bool mail_confirmed { get; set; }
        public bool enabled { get; set; }
        public DateTime register_date { get; set; }
    }
}

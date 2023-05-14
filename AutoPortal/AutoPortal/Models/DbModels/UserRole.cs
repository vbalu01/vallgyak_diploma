using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("userroles")]
    public class UserRole
    {
        [Required]
        public int userId { get; set; }
        [Required]
        public string roleId { get; set; }
    }
}

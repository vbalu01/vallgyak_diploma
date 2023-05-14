using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Required]
        public string role { get; set; }
    }
}

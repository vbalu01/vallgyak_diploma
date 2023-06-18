using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("users")]
    public partial class User
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public DateTime register_date { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
    }

    public partial class User
    {
        public List<Role> GetRoles()
        {
            List<Role> roles = new();
            using(SQL mysql = new SQL())
            {
                foreach(UserRole ur in mysql.userRoles.Where(ur=>ur.userId == this.id))
                {
                    using(SQL mysqll = new SQL())
                    {
                        roles.Add(mysqll.roles.SingleOrDefault(r => r.role == ur.roleId));
                    }
                }
            }
            return roles;
        }
    }
}

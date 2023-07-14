using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("services")]
    public partial class Service
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
    }

    public partial class Service
    {
        public static string GetServiceNameById(int id)
        {
            using(SQL mysql = new SQL())
            {
                return mysql.services.SingleOrDefault(s => s.id == id).name;
            }
        }

        public bool isValid()
        {
            if (this.status.HasFlag(eAccountStatus.EMAIL_CONFIRM) && this.status.HasFlag(eAccountStatus.ADMIN_CONFIRM) && !this.status.HasFlag(eAccountStatus.DISABLED) && !this.status.HasFlag(eAccountStatus.BANNED))
                return true;
            return false;
        }
    }
}

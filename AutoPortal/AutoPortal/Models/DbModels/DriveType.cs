using AutoPortal.Libs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("drivetypes")]
    public partial class DriveType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string drive { get; set; }
    }

    public partial class DriveType
    {
        public static int? findIdByDriveType(string drive)
        {
            using(SQL mysql = new SQL())
            {
                return mysql.driveTypes.SingleOrDefault(d => d.drive == drive).id;
            }
        }

        public static string findDriveTypeById(int id)
        {
            using (SQL mysql = new SQL())
            {
                return mysql.driveTypes.SingleOrDefault(d => d.id == id).drive;
            }
        }
    }
}

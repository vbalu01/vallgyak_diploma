using AutoPortal.Libs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("mileagestands")]
    public partial class MileageStand
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public int mileage { get; set; }
        [Required]
        public DateTime date { get; set; }
    }

    public partial class MileageStand
    {
        public static List<MileageStand> GetVehicleMileageStands(string vehicle_id)
        {
            using(SQL mysql = new SQL())
            {
                return mysql.mileageStands.Where(tmp=>tmp.vehicle_id == vehicle_id).ToList();
            }
        }
    }
}

using AutoPortal.Libs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("fueltypes")]
    public partial class FuelType
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string fuel { get; set; }
    }

    public partial class FuelType
    {
        public static int? findIdByFuel(string fuel)
        {
            using(SQL mysql = new SQL())
            {
                return mysql.fuelTypes.SingleOrDefault(f=>f.fuel == fuel).id;
            }
        }

        public static string findFuelById(int id)
        {
            using (SQL mysql = new SQL())
            {
                return mysql.fuelTypes.SingleOrDefault(f => f.id == id).fuel;
            }
        }
    }
}

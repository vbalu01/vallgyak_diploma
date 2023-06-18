using AutoPortal.Libs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehiclecategories")]
    public partial class VehicleCategory
    {
        [Key]
        public int id { get; set; }
        [Required]
        public string category { get; set; }
    }

    public partial class VehicleCategory
    {
        public static int? findIdByCategory(string category)
        {
            using(SQL mysql = new SQL())
            {
                return mysql.vehicleCategories.SingleOrDefault(c=>c.category == category).id;
            }
        }

        public static string findCategoryById(int id)
        {
            using (SQL mysql = new SQL())
            {
                return mysql.vehicleCategories.SingleOrDefault(c => c.id == id).category;
            }
        }
    }
}

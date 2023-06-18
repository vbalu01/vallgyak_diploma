using AutoPortal.Libs;
using AutoPortal.Models.RequestModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("othercosts")]
    public partial class OtherCost
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public string title { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public int cost { get; set; }
        [Required]
        public DateTime date { get; set; }
    }

    public partial class OtherCost
    {
        public OtherCost() { }
        public OtherCost(AddNewCostModel m)
        {
            this.id = Guid.NewGuid();
            this.vehicle_id = m.vehicle_id;
            this.title = m.title;
            this.description = m.description;
            this.cost = m.cost;
            this.date = m.date;
        }

        public static List<OtherCost> GetVehicleOtherCosts(string vehicleId)
        {
            using(SQL mysql = new SQL())
            {
                return mysql.otherCosts.Where(tmp=>tmp.vehicle_id == vehicleId).ToList();
            }
        }
    }
}

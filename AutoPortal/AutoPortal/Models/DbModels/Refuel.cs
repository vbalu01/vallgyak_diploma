using AutoPortal.Libs;
using AutoPortal.Models.RequestModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("refuels")]
    public partial class Refuel
    {
        [Key]
        [Required]
        public Guid id { get; set; }
        [Required]
        public string vehicle_id { get; set; }
        [Required]
        public int refuel_cost { get; set; }
        [Required]
        public int traveled_distance { get; set; }
        [Required]
        public int amount_of_fuel { get; set; }
        [Required]
        public bool premium_fuel { get; set; }
        [Required]
        public DateTime fueling_date { get; set; }
        public bool? archive {  get; set; }
    }

    public partial class Refuel
    {
        public Refuel() { }
        public Refuel(AddNewRefuelModel m)
        {
            this.id = Guid.NewGuid();
            this.vehicle_id = m.vehicle_id;
            this.refuel_cost = m.refuel_cost;
            this.traveled_distance = m.traveled_distance;
            this.amount_of_fuel = m.amount_of_fuel;
            this.premium_fuel = m.premium_fuel;
            this.fueling_date = m.fueling_date;
        }

        public static List<Refuel> GetVehicleRefuels(string vehicle_id){
            using(SQL mysql = new SQL())
            {
                return mysql.refuels.Where(tmp=>tmp.vehicle_id == vehicle_id && (!tmp.archive.HasValue || (tmp.archive.HasValue && !(bool)tmp.archive))).ToList();
            }
        }
        public static List<Refuel> GetVehicleRefuelsAdmin(string vehicle_id)
        {
            using (SQL mysql = new SQL())
            {
                return mysql.refuels.Where(tmp => tmp.vehicle_id == vehicle_id).ToList();
            }
        }
    }
}

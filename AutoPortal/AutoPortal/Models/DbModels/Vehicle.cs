using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.RequestModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehicles")]
    public partial class Vehicle
    {
        [Key]
        [Required]
        public string chassis_number { get; set; }
        [Required]
        public string engine_number { get; set; }
        public string license { get; set; }
        [Required]
        public string engine_code { get; set; }
        [Required]
        public int category { get; set; }
        [Required]
        public int manufact_year { get; set; }
        [Required]
        public string make { get; set; }
        [Required]
        public string model { get; set; }
        public string modeltype { get; set; }
        [Required]
        public int fuel { get; set; }
        [Required]
        public int transmission { get; set; }
        [Required]
        public int num_of_gears { get; set; }
        [Required]
        public int drive { get; set; }
        public int engine_ccm { get; set; }
        [Required]
        public int performance { get; set; }
        [Required]
        public int torque {get; set; }
        public string body { get; set; }
        [Required]
        public int num_of_doors { get; set; }
        [Required]
        public int num_of_seats { get; set; }
        [Required]
        public int weight { get; set; }
        [Required]
        public int max_weight { get; set; }
    }

    public partial class Vehicle
    {
        public Vehicle() { }
        public Vehicle(AddUserCarModel m)
        {
            this.chassis_number = m.chassis_number;
            this.engine_number = m.engine_number;
            this.license  = m.license;
            this.engine_code = m.engine_code;
            this.category = Convert.ToInt32(m.category);
            this.manufact_year = Convert.ToInt32(m.manufact_year);
            this.make = m.make;
            this.model = m.model;
            this.modeltype = m.modelType;
            this.fuel = Convert.ToInt32(m.fuel);
            this.transmission = Convert.ToInt32(m.transmission);
            this.drive = Convert.ToInt32(m.drive);
            this.engine_ccm = Convert.ToInt32(m.engine_ccm);
            this.performance = Convert.ToInt32(m.performance);
            this.torque = Convert.ToInt32(m.torque);
            this.body = m.body;
            this.num_of_doors = Convert.ToInt32(m.num_of_doors);
            this.num_of_seats = Convert.ToInt32(m.num_of_seats);
            this.weight = Convert.ToInt32(m.weight);
            this.max_weight = Convert.ToInt32(m.max_weight);
        }

        public List<MileageStandModel> getMileageStands()
        {
            List<MileageStandModel> stands = new List<MileageStandModel>();
            
            using(SQL mysql = new SQL())
            {
                //Felhasználó által rögzített
                foreach (MileageStand s in mysql.mileageStands.Where(st=>st.vehicle_id == this.chassis_number))
                {
                    stands.Add(new MileageStandModel() { MileageStand = s.mileage, RecordedDate = s.date, MileageStandType = eMileageStandType.USER_RECORDED });
                }
                //Szerviz adatok
                foreach(ServiceEvent s in mysql.serviceEvents.Where(se=>se.vehicle_id == this.chassis_number))
                {
                    stands.Add(new MileageStandModel() { MileageStand = s.mileage, RecordedDate = s.date, MileageStandType = eMileageStandType.SERVICE_RECORDED });
                }
            }

            return stands;
        }
    }
}

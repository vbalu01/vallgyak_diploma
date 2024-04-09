using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AdminUpdateVehicleModel
    {
        [Required]
        public string chassis_number {  get; set; }
        public string license {  get; set; }
        public string make {  get; set; }
        public string model {  get; set; }
        public string modeltype {  get; set; }
        public int manufactyear {  get; set; }
        public int category { get; set; }
        public string body { get; set; }
        public int numofdoors { get; set; }
        public int numofseats { get; set; }
        public int weight { get; set; }
        public int maxweight { get; set; }
        public string enginenumber { get; set; }
        public string enginecode { get; set; }
        public int fueltype { get; set; }
        public int engineccm { get; set; }
        public int performance { get; set; }
        public int torque { get; set; }
        public int transmissiontype { get; set; }
        public int numofgears { get; set; }
        public int drivetype { get; set; }
    }
}

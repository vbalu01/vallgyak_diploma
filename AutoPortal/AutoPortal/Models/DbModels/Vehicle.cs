using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoPortal.Models.DbModels
{
    [Table("vehicles")]
    public class Vehicle
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
        public int maufact_year { get; set; }
        [Required]
        public string make { get; set; }
        [Required]
        public string model { get; set; }
        public string modelType { get; set; }
        [Required]
        public int fuel { get; set; }
        [Required]
        public int transmission { get; set; }
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
}

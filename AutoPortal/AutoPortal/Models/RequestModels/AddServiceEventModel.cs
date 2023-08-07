using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AddServiceEventModel
    {
        [Required]
        public string vehicleId { get; set; }
        [Required]
        public string serviceTitle { get; set; }
        [Required]
        public string serviceDescription { get; set; }
        [Required]
        public int? serviceCost { get; set; }
        [Required]
        public DateTime serviceDate { get; set; }
        [Required]
        public int? serviceMileage { get; set; }
        [Required]
        public eServiceType serviceType { get; set; }
        [Required]
        public string serviceComment { get; set; }
    }
}

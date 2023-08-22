using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AddServiceEventAdminMondel
    {
        [Required]
        public string vehicleId { get; set; }
        [Required]
        public int? newServiceService { get; set; }
        [Required]
        public string newServiceTitle { get; set; }
        [Required]
        public string newServiceDescription { get; set; }
        public string newServiceOther { get; set; }
        [Required]
        public int? newServiceCost { get; set; }
        [Required]
        public DateTime newServiceDate { get; set; }
        [Required]
        public int? newServiceMileage { get; set; }
        [Required]
        public eServiceType newServiceType { get; set; }
    }
}

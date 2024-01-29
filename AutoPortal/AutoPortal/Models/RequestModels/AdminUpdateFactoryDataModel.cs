using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AdminUpdateFactoryDataModel
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
    }
}

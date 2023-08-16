using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AdminUpdateServiceDataModel
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string name { get; set; }
        [Required]  
        public string phone { get; set; }
        [Required]
        public string country { get; set; }
        [Required]
        public string city { get; set; }
        [Required]
        public string address { get; set; }
        public string website { get; set; }
        public string description { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class UpdateCompanyDataModel
    {
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
        [Required]
        public string description { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class RegisterModel
    {
        [Required]
        public string email { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string password { get; set; }
        [Required]
        public string repassword { get; set; }
    }

    public class CompanyRegisterModel : RegisterModel
    {
        [Required]
        public string phone { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public bool regType { get; set; } //True - Service, False - Dealer
    }
}

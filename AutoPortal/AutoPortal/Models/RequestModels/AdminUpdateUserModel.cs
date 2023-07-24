using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AdminUpdateUserModel
    {
        [Required]
        public int id { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string userName { get; set; }
        [Required]
        public eAccountStatus status { get; set; }
        [Required]
        public List<string> roles { get; set; }
    }
}

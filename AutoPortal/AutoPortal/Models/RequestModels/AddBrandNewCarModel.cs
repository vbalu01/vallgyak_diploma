using AutoPortal.Models.AppModels;
using System.ComponentModel.DataAnnotations;

namespace AutoPortal.Models.RequestModels
{
    public class AddBrandNewCarModel : AddUserCarModel
    {
        [Required]
        public string email { get; set; }
    }
}

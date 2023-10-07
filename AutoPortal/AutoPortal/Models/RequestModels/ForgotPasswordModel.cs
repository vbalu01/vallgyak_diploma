using AutoPortal.Models.AppModels;

namespace AutoPortal.Models.RequestModels
{
    public class ForgotPasswordModel
    {
        public string newPassword { get; set; }
        public string newPasswordRepeat { get; set; }
        public string token { get; set; }
        public int userId { get; set; }
        public eVehicleTargetTypes userType { get; set; }
    }
}

using System.ComponentModel;

namespace AutoPortal.Models.AppModels
{
    [Flags]
    public enum eAccountStatus
    {
        [Description("-")]
        None = 0,
        [Description("Email megerősítve")]
        EMAIL_CONFIRM = 1, //Email megerősítve
        [Description("Adminisztrátori jóváhagyás")]
        ADMIN_CONFIRM = 2, //Admin visszaigazolás
        [Description("Kitiltva")]
        BANNED = 4, //Admin által tiltva
        [Description("Szüneteltetve")]
        DISABLED = 8 //Tulaj által tiltva
    }
}
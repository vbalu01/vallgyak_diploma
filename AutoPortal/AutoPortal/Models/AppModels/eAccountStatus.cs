namespace AutoPortal.Models.AppModels
{
    [Flags]
    public enum eAccountStatus
    {
        None = 0,
        EMAIL_CONFIRM = 1, //Email megerősítve
        ADMIN_CONFIRM = 2, //Admin visszaigazolás
        BANNED = 4, //Admin által tiltva
        DISABLED = 8 //Tulaj által tiltva
    }
}
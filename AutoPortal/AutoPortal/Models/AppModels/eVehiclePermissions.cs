using System.ComponentModel;

namespace AutoPortal.Models.AppModels
{
    public enum eVehiclePermissions
    {
        [Description("-")]
        NONE = 0,
        [Description("Tulajdonos")]
        OWNER = 1,
        [Description("Üzembentartó")]
        SUBOWNER = 2,
        [Description("Sofőr")]
        DRIVER = 3,
        [Description("Kereskedő")]
        DEALER = 4
    }
}

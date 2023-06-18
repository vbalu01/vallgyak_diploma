using System.ComponentModel;

namespace AutoPortal.Models.AppModels
{
    public enum eServiceType
    {
        [Description("Egyéb")]
        OTHER = 0, //Egyéb
        [Description("Éves szerviz")]
        ANNUAL_SERVICE = 1, //Éves szerviz
        [Description("Rendszeres szerviz")]
        REGULAR_SERVICE = 2, //Rendszeres karbantartás
        [Description("Garanciális szerviz")]
        WARRANTY_SERVICE = 3, //Garanciális szerviz
        [Description("Kerék csere")]
        WHEEL_CHANGE = 4, //Kerék csere
        [Description("Eredetiség vizsgálat")]
        AUTHENTICITY_CHECK = 5, //Eredetiségvizsgálat
        [Description("Műszaki vizsga")]
        TECHNICAL_EXAMINATION = 6 //Műszaki vizsga
    }
}

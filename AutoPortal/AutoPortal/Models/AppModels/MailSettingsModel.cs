namespace AutoPortal.Models.AppModels
{
    public static class MailSettingsModel
    {
        public static string username { get; set; }
        public static string password { get; set; }
        public static string host { get; set; }
        public static int port { get; set; }
        public static bool useSSL { get; set; }
        public static string defaultSender { get; set; }
    }
}

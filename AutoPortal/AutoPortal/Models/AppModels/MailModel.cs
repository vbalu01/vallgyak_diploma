namespace AutoPortal.Models.AppModels
{
    public class MailModel
    {
        public string from { get; set; } = MailSettingsModel.defaultSender;
        public string to { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public bool isHtml { get; set; } = false;
    }
}

using AutoPortal.Models.AppModels;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Text;

namespace AutoPortal.Libs
{
    public static class MailSender
    {
        public async static Task<bool> SendMail(MailModel m)
        {
            MailMessage message = new MailMessage(m.from, m.to) {
                Subject = m.subject,
                Body = m.body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = m.isHtml
            };

            SmtpClient client = new SmtpClient(MailSettingsModel.host, MailSettingsModel.port) {
                EnableSsl = MailSettingsModel.useSSL,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(MailSettingsModel.username, MailSettingsModel.password)
            }; 
            try {
                Log.LogMessageAsync("SendMail: " + JsonConvert.SerializeObject(m), null);
                client.Send(message);
                return true;
            }catch (Exception ex) {
                Log.ErrorLog(ex.Message);
                return false;
            }
        }
    }
}

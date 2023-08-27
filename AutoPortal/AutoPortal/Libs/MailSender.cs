using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
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
                Functions.WriteLog("SendMail: " + JsonConvert.SerializeObject(m));
                client.Send(message);
                return true;
            }catch (Exception ex) {
                Functions.WriteErrorLog(ex.Message);
                return false;
            }
        }

        public async static Task<bool> SendSuccessRegisterMail(dynamic u, Token t, string host)
        {
            MailModel m = new()
            {
                subject = "AutoPortal - Regisztráció",
                from = "noreply@autoportal.hu",
                isHtml = true,
                to = u.email,
                body = $"<p><strong>Tisztelt {u.name}!</strong></p>\r\n\r\n<p>Köszönjük, hogy regisztrált az AutoPortal oldalára, felhasználói fiókját sikeresen létrehoztuk.</p>\r\n\r\n<p>Kérjük, hogy az alkalmazás korlátok nélküli használata érdekében erősítse meg az e-mail címét <a href=\"http://{host}/Token/ConfirmRegistration?token={t.token}\">ide</a> kattintva, vagy másolja be a böngészőbe az alábbi linket: </p>\r\n\r\n<p><a href=\"http://{host}/Token/ConfirmRegistration?token={t.token}\">http://{host}/Token/ConfirmRegistration?token={t.token}</a></p>\r\n\r\n<p><strong>Az AutoPortal csapata</strong></p>\r\n"
            };
            return await SendMail(m);
        }
    }
}

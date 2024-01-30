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
                client.SendAsync(message, null);
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

        public async static Task<bool> SendNewPasswordMail(string email, Token t, string host)
        {
            MailModel m = new()
            {
                subject = "AutoPortal - Elfelejtett jelszó",
                from = "noreply@autoportal.hu",
                isHtml = true,
                to = email,
                body = $"<p><strong>Tisztelt felhasználónk!</strong></p>\r\n\r\n<p>Az új jelszavának megadásához kattintson <a href=\"http://{host}/Token/ForgotPassword?token={t.token}\">ide</a>, vagy másolja be a böngészőbe az alábbi linket: </p>\r\n\r\n<p><a href=\"http://{host}/Token/ForgotPassword?token={t.token}\">http://{host}/Token/ForgotPassword?token={t.token}</a></p>\r\n\r\n<p><strong>Üdvözlettel: az AutoPortal csapata</strong></p>\r\n"
            };
            return await SendMail(m);
        }

        public async static Task<bool> SendFactoryRegisterMail(string email, string name, string password)
        {
            MailModel m = new()
            {
                subject = "AutoPortal - Gyártó regisztráció",
                from = "noreply@autoportal.hu",
                isHtml = true,
                to = email,
                body = $"<p><strong>Tisztelt {name}!</strong></p>\r\n\r\n<p>Gyártó regisztráció történt a megadott email címre: {email}. Az API-ba történő belépéshez szükséges jelszava a következő: <b>{password}. <label style='color:red;'>Kérjük hogy a jelszót az első bejelentkezés után változtassák meg a fejlesztői dokumentációban leírtak szerint.</label></p><p>Üdvözlettel: Az AutoPortal csapata</p>"
            };
            return await SendMail(m);
        }

        public async static Task<bool> SendAdminNewFactoryPwdMail(string email, string name, string password)
        {
            MailModel m = new()
            {
                subject = "AutoPortal - Új jelszó igénylés",
                from = "noreply@autoportal.hu",
                isHtml = true,
                to = email,
                body = $"<p><strong>Tisztelt {name}!</strong></p>\r\n\r\n<p>Admin általi gyártó jelszó módosítás történt a megadott email címen: {email}. Az API-ba történő belépéshez szükséges új jelszava a következő: <b>{password}. <label style='color:red;'>Kérjük hogy a jelszót az első bejelentkezés után változtassák meg a fejlesztői dokumentációban leírtak szerint.</label></p><p>Üdvözlettel: Az AutoPortal csapata</p>"
            };
            return await SendMail(m);
        }

        public async static Task<bool> SendNewFactoryPwdMail(string email, string name, string password)
        {
            MailModel m = new()
            {
                subject = "AutoPortal - Új jelszó igénylés",
                from = "noreply@autoportal.hu",
                isHtml = true,
                to = email,
                body = $"<p><strong>Tisztelt {name}!</strong></p>\r\n\r\n<p>Gyártói jelszó módosítás történt a megadott email címen: {email}. Az API-ba történő belépéshez szükséges új jelszava a következő: <b>{password}. <label style='color:red;'>Kérjük hogy a jelszót az első bejelentkezés után változtassák meg a fejlesztői dokumentációban leírtak szerint.</label></p><p>Üdvözlettel: Az AutoPortal csapata</p>"
            };
            return await SendMail(m);
        }
    }
}

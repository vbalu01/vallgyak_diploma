using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using NToastNotify;

namespace AutoPortal.Controllers
{
    public class TokenController : BaseController
    {
        public TokenController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        { }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ConfirmRegistration(string token)
        {
            if (token == null)
            {
                this._Notification.AddErrorToastMessage("Hibás adatok!");
                return Redirect("/Auth/Login");
            }

            Token t = this._SQL.tokens.SingleOrDefault(tt => tt.token == token);
            if (t == null)
            {
                this._Notification.AddErrorToastMessage("Hibás kulcs!");
                return Redirect("/Auth/Login");
            }

            if (t.token_type != eTokenType.MAIL_CONFIRM)
            {
                this._Notification.AddErrorToastMessage("Hibás kulcs típus!");
                return Redirect("/Auth/Login");
            }

            if (t.expire <= DateTime.Now)
            {
                this._Notification.AddErrorToastMessage("A kulcs már lejárt!");
                return Redirect("/Auth/Login");
            }

            if (!t.available)
            {
                this._Notification.AddErrorToastMessage("A kulcs nem elérhető!");
                return Redirect("/Auth/Login");
            }

            switch (t.target_type)
            {
                case eVehicleTargetTypes.DEALER:
                    if(!_SQL.dealers.Any(d=>d.id == t.target_id))
                    {
                        this._Notification.AddErrorToastMessage("A felhasználó nem található!");
                        return Redirect("/Auth/Login");
                    }
                    Dealer d = _SQL.dealers.Single(dd => dd.id == t.target_id);
                    if (d.status.HasFlag(eAccountStatus.EMAIL_CONFIRM))
                    {
                        this._Notification.AddInfoToastMessage("Az E-Mail cím már meg lett erősítve!");
                        return Redirect("/Auth/Login");
                    }
                    d.status = d.status | eAccountStatus.EMAIL_CONFIRM;
                    _SQL.dealers.Update(d);
                    break;
                case eVehicleTargetTypes.USER:
                    if (!_SQL.users.Any(usr => usr.id == t.target_id))
                    {
                        this._Notification.AddErrorToastMessage("A felhasználó nem található!");
                        return Redirect("/Auth/Login");
                    }
                    User u = _SQL.users.Single(usr => usr.id == t.target_id);
                    if (u.status.HasFlag(eAccountStatus.EMAIL_CONFIRM))
                    {
                        this._Notification.AddInfoToastMessage("Az E-Mail cím már meg lett erősítve!");
                        return Redirect("/Auth/Login");
                    }
                    u.status = u.status | eAccountStatus.EMAIL_CONFIRM;
                    _SQL.users.Update(u);
                    break;
                case eVehicleTargetTypes.SERVICE:
                    if (!_SQL.services.Any(se => se.id == t.target_id))
                    {
                        this._Notification.AddErrorToastMessage("A felhasználó nem található!");
                        return Redirect("/Auth/Login");
                    }
                    Service s = _SQL.services.Single(se => se.id == t.target_id);
                    if (s.status.HasFlag(eAccountStatus.EMAIL_CONFIRM))
                    {
                        this._Notification.AddInfoToastMessage("Az E-Mail cím már meg lett erősítve!");
                        return Redirect("/Auth/Login");
                    }
                    s.status = s.status | eAccountStatus.EMAIL_CONFIRM;
                    _SQL.services.Update(s);
                    break;
                case eVehicleTargetTypes.FACTORY:
                    if (!_SQL.factories.Any(fa => fa.id == t.target_id))
                    {
                        this._Notification.AddErrorToastMessage("A felhasználó nem található!");
                        return Redirect("/Auth/Login");
                    }
                    Factory f = _SQL.factories.Single(fa => fa.id == t.target_id);
                    if (f.status.HasFlag(eAccountStatus.EMAIL_CONFIRM))
                    {
                        this._Notification.AddInfoToastMessage("Az E-Mail cím már meg lett erősítve!");
                        return Redirect("/Auth/Login");
                    }
                    f.status = f.status | eAccountStatus.EMAIL_CONFIRM;
                    _SQL.factories.Update(f);
                    break;
            }

            this._SQL.tokens.Remove(t);

            this._SQL.SaveChangesAsync();

            this._Notification.AddSuccessToastMessage("Sikeres megerősítés!");
            return Redirect("/Auth/Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword(string token)
        {
            if (token == null)
            {
                this._Notification.AddErrorToastMessage("Hibás adatok!");
                return Redirect("/Auth/AskNewPassword");
            }
            if (this.loginType != eVehicleTargetTypes.NONE)
            {
                _Notification.AddErrorToastMessage("A felhasználó be van jelentkezve!");
                return Redirect("/Home/");
            }

            Token t = this._SQL.tokens.SingleOrDefault(tt => tt.token == token);
            if (t == null)
            {
                this._Notification.AddErrorToastMessage("Hibás kulcs!");
                return Redirect("/Auth/AskNewPassword");
            }

            if (t.token_type != eTokenType.ASK_NEW_PASSWORD)
            {
                this._Notification.AddErrorToastMessage("Hibás kulcs típus!");
                return Redirect("/Auth/AskNewPassword");
            }

            if (t.expire <= DateTime.Now)
            {
                this._Notification.AddErrorToastMessage("A kulcs már lejárt!");
                return Redirect("/Auth/AskNewPassword");
            }

            if (!t.available)
            {
                this._Notification.AddErrorToastMessage("A kulcs nem elérhető!");
                return Redirect("/Auth/AskNewPassword");
            }

            TempData["userType"] = t.target_type;
            TempData["userId"] = t.target_id;
            TempData["token"] = token;

            return Redirect("/Auth/ForgotPassword");
        }
    }
}

using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.DbModels;
using AutoPortal.Models.RequestModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NToastNotify;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using AutoPortal.Models.AppModels;
using Newtonsoft.Json.Linq;

namespace AutoPortal.Controllers
{
    public class AuthController : BaseController
    {
        public AuthController(IConfiguration config, SQL sql, IToastNotification notification) : base(config, sql, notification)
        { }

        #region Views
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register() 
        {
            return View();
        }

        public IActionResult AskNewPassword()
        {
            if(this.loginType != eVehicleTargetTypes.NONE)
            {
                _Notification.AddErrorToastMessage("A felhasználó be van jelentkezve!");
                return Redirect("/Home/");
            }
            return View();
        }

        public IActionResult ForgotPassword()
        {
            if (this.loginType != eVehicleTargetTypes.NONE)
            {
                _Notification.AddErrorToastMessage("A felhasználó be van jelentkezve!");
                return Redirect("/Home/");
            }
            if (TempData["token"] != null && TempData["userId"] != null && TempData["userType"] != null)
            {
                return View();
            }
            else
            {
                this._Notification.AddErrorToastMessage("Hiba lépett fel! - Elfelejtett Jelszó");
                return View("Login");
            }
        }

        #endregion

        #region Register
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody]RegisterModel m)
        {
            if (string.IsNullOrEmpty(m.name) || string.IsNullOrEmpty(m.email) || string.IsNullOrEmpty(m.password) || string.IsNullOrEmpty(m.repassword)) {
                resp.Success = false;
                resp.Message = "Bad data fill";
                _Notification.AddErrorToastMessage("Minden adatot ki kell tölteni!");
                return BadRequest(resp.ToString());
            }

            if(m.password.Length < 6) {
                resp.Success = false;
                resp.Message = "Passwords must be at least 6 chars!";
                _Notification.AddErrorToastMessage("A jelszónak min. 6 karakterből kell állnia!");
                return BadRequest(base.resp.ToString());
            }

            if(m.password != m.repassword) {
                resp.Success = false;
                resp.Message = "Passwords not match!";
                _Notification.AddErrorToastMessage("A jelszavak nem egyeznek!");
                return BadRequest(base.resp.ToString());
            }
            
            if(_SQL.users.Any(u=>u.email == m.email)) {
                resp.Success = false;
                resp.Message = "Email already taken.";
                _Notification.AddErrorToastMessage("Az Email már használatban van!");
                return BadRequest(resp.ToString());
            }

            User u = new User() { 
                email = m.email,
                password = PasswordManager.GenerateHash(m.password),
                name = m.name,
                register_date = DateTime.Now,
                status = eAccountStatus.None
            };

            _SQL.users.Add(u);
            _SQL.SaveChanges();

            Token t = TokenHandler.GenerateMailConfirmToken(u.id, eVehicleTargetTypes.USER);
            _SQL.tokens.Add(t);
            _SQL.SaveChanges();

            MailSender.SendSuccessRegisterMail(u, t, this.Request.Host.ToString());

            _Notification.AddSuccessToastMessage("Sikeres regisztráció!");
            resp.Message = "Register success!";
            return Ok(resp.ToString());
        }

        [HttpPost]
        public async Task<IActionResult> RegisterCompany([FromBody] CompanyRegisterModel m)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(m.name) || string.IsNullOrEmpty(m.email) || string.IsNullOrEmpty(m.password) || string.IsNullOrEmpty(m.repassword) || string.IsNullOrEmpty(m.phone) || string.IsNullOrEmpty(m.description))
                {
                    resp.Success = false;
                    resp.Message = "Bad data fill";
                    _Notification.AddErrorToastMessage("Minden adatot ki kell tölteni!");
                    return BadRequest(resp.ToString());
                }

                if (m.password.Length < 6)
                {
                    resp.Success = false;
                    resp.Message = "Passwords must be at least 6 chars!";
                    _Notification.AddErrorToastMessage("A jelszónak min. 6 karakterből kell állnia!");
                    return BadRequest(base.resp.ToString());
                }

                if (m.password != m.repassword)
                {
                    resp.Success = false;
                    resp.Message = "Passwords not match!";
                    _Notification.AddErrorToastMessage("A jelszavak nem egyeznek!");
                    return BadRequest(base.resp.ToString());
                }

                if (_SQL.users.Any(u => u.email == m.email) || _SQL.services.Any(s=>s.email == m.email) || _SQL.dealers.Any(d=>d.email == m.email))
                {
                    resp.Success = false;
                    resp.Message = "Email already taken.";
                    _Notification.AddErrorToastMessage("Az Email már használatban van!");
                    return BadRequest(resp.ToString());
                }

                int regId = -1;
                eVehicleTargetTypes type = eVehicleTargetTypes.NONE;

                if (m.regType) { //Szerviz
                    type = eVehicleTargetTypes.SERVICE;
                    Service s = new Service()
                    {
                        email = m.email,
                        description = m.description,
                        name = m.name,
                        password = PasswordManager.GenerateHash(m.password),
                        phone = m.phone,
                        status = eAccountStatus.None
                    };
                    _SQL.services.Add(s);
                    await _SQL.SaveChangesAsync();
                    regId = s.id;
                } else { //Kereskedő
                    type = eVehicleTargetTypes.DEALER;
                    Dealer d = new Dealer()
                    {
                        email = m.email,
                        description = m.description,
                        name = m.name,
                        password = PasswordManager.GenerateHash(m.password),
                        phone = m.phone,
                        status = eAccountStatus.None
                    };
                    _SQL.dealers.Add(d);
                    await _SQL.SaveChangesAsync();
                    regId = d.id;
                }

                Token t = TokenHandler.GenerateMailConfirmToken(regId, type);
                _SQL.tokens.Add(t);
                _SQL.SaveChanges();

                MailSender.SendSuccessRegisterMail(regId, t, this.Request.Host.ToString());

                _Notification.AddSuccessToastMessage("Sikeres regisztráció!");
                resp.Message = "Register success!";
                return Ok(resp.ToString());
            }
            else {
                resp.Success = false;
                resp.Message = "Bad data fill";
                _Notification.AddErrorToastMessage("Minden adatot ki kell tölteni!");
                return BadRequest(resp.ToString());
            }
        }
        #endregion

        #region Login

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            eVehicleTargetTypes loginType = getLoginMethod(email);
            if(loginType == eVehicleTargetTypes.NONE) { //Hibás bejelentkezés
                _Notification.AddInfoToastMessage("Hibás felhasználónév, vagy jelszó!");
                return View();
            }
            else if(loginType == eVehicleTargetTypes.FACTORY) { //Gyártói bejelentkezés
                _Notification.AddWarningToastMessage("Gyártói bejelentkezés csak API-n keresztül engedélyezett!");
                return View();
            } else if (loginType == eVehicleTargetTypes.USER) { //Felhasználói bejelentkezés
                User user = await this._SQL.users.SingleAsync(u => u.email == email);

                if (PasswordManager.AreEqual(password, user.password))
                {
                    
                    if (!user.status.HasFlag(eAccountStatus.EMAIL_CONFIRM)) {
                        this._Notification.AddErrorToastMessage("A felhasználó nem elérhető!");
                        return View();
                    }

                    if (user.status.HasFlag(eAccountStatus.BANNED))
                    {
                        _Notification.AddErrorToastMessage("A felhasználót egy admin letiltotta!");
                        return View();
                    }

                    ClaimsPrincipal princ = this.GenerateUserClaim(user, this._SQL.userRoles.Where(a => a.userId == user.id).Select(a => a.roleId));
                    await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princ, new AuthenticationProperties()
                    {
                        IsPersistent = true
                    });
                    this._Notification.AddSuccessToastMessage("A bejelentkezés sikeres!");
                    return Redirect("/Home");
                }
                else
                {
                    this._Notification.AddInfoToastMessage("Hibás felhasználónév, vagy jelszó!");
                    return View();
                }
            } else if(loginType == eVehicleTargetTypes.SERVICE) { //Szerviz bejelentkezés
                Service service = await this._SQL.services.SingleAsync(s => s.email == email);

                if (PasswordManager.AreEqual(password, service.password))
                {
                    if (service.status.HasFlag(eAccountStatus.BANNED)) {
                        this._Notification.AddErrorToastMessage("A felhasználó kitiltva!");
                        return View();
                    }

                    if (!service.status.HasFlag(eAccountStatus.EMAIL_CONFIRM)) {
                        this._Notification.AddWarningToastMessage("Bejelentkezéshez E-mail megerősítés szükséges!");
                        return View();
                    }

                    ClaimsPrincipal princ = this.GenerateServiceClaim(service);
                    await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princ, new AuthenticationProperties()
                    {
                        IsPersistent = true
                    });
                    this._Notification.AddSuccessToastMessage("A bejelentkezés sikeres!");
                    return Redirect("/Home");
                }
                else
                {
                    this._Notification.AddInfoToastMessage("Hibás felhasználónév, vagy jelszó!");
                    return View();
                }
            } else if(loginType == eVehicleTargetTypes.DEALER) {
                Dealer dealer = await this._SQL.dealers.SingleAsync(s => s.email == email);

                if (PasswordManager.AreEqual(password, dealer.password))
                {
                    if (dealer.status.HasFlag(eAccountStatus.BANNED))
                    {
                        this._Notification.AddErrorToastMessage("A felhasználó kitiltva!");
                        return View();
                    }

                    if (!dealer.status.HasFlag(eAccountStatus.EMAIL_CONFIRM))
                    {
                        this._Notification.AddWarningToastMessage("Bejelentkezéshez E-mail megerősítés szükséges!");
                        return View();
                    }

                    ClaimsPrincipal princ = this.GenerateDealerClaim(dealer);
                    await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princ, new AuthenticationProperties()
                    {
                        IsPersistent = true
                    });
                    this._Notification.AddSuccessToastMessage("A bejelentkezés sikeres!");
                    return Redirect("/Home");
                }
                else
                {
                    this._Notification.AddInfoToastMessage("Hibás felhasználónév, vagy jelszó!");
                    return View();
                }
            }

            this._Notification.AddErrorToastMessage("AUTH ERROR");
            return View();

        }

        private eVehicleTargetTypes getLoginMethod(string email) {
            if(_SQL.users.Any(u=>u.email == email)) { //Felhasználói bejelentkezés
                return eVehicleTargetTypes.USER;
            }else if(_SQL.factories.Any(f=>f.email == email)) { //Gyártói bejelentkezés
                return eVehicleTargetTypes.FACTORY;
            }else if(_SQL.services.Any(s=>s.email == email)) { //Szerviz bejelenetkezés
                return eVehicleTargetTypes.SERVICE;
            }else if(_SQL.dealers.Any(d=>d.email == email)) { //Kereskedői bejelentkezés
                return eVehicleTargetTypes.DEALER;
            }
            return eVehicleTargetTypes.NONE; //Nem létezik a fiók
        }

        private ClaimsPrincipal GenerateUserClaim(User user, IEnumerable<string> Roles)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.email),
                new Claim("UserId", user.id.ToString()),
                new Claim("LoginType", "USER"),
                new Claim(ClaimTypes.Role, "user")
            };
            foreach (string role in Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        private ClaimsPrincipal GenerateServiceClaim(Service s)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, s.email),
                new Claim("UserId", s.id.ToString()),
                new Claim("LoginType", "SERVICE"),
                new Claim(ClaimTypes.Role, "service")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }

        private ClaimsPrincipal GenerateDealerClaim(Dealer d)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, d.email),
                new Claim("UserId", d.id.ToString()),
                new Claim("LoginType", "DEALER"),
                new Claim(ClaimTypes.Role, "dealer")
            };

            return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        }
        #endregion

        #region Logout
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Auth/Login");
        }

        #endregion

        #region AskNewPassword

        [HttpPost]
        public async Task<IActionResult> AskNewPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                this._Notification.AddErrorToastMessage("A megadott E-Mail cím érvénytelen!");
                return View();
            }
            eVehicleTargetTypes targetType = getLoginMethod(email);
            if(targetType == eVehicleTargetTypes.NONE)
            {
                this._Notification.AddErrorToastMessage("Ezzel az e-mail címmel nem található felhasználó!");
                return View();
            }
            else if(targetType == eVehicleTargetTypes.FACTORY)
            {
                this._Notification.AddErrorToastMessage("Gyártói művelet letiltva!");
                return View();
            }
            else
            {
                int targetId = -1;
                switch (targetType)
                {
                    case eVehicleTargetTypes.USER:
                        targetId = _SQL.users.Single(u => u.email == email).id; 
                        break;
                    case eVehicleTargetTypes.DEALER:
                        targetId = _SQL.dealers.Single(u => u.email == email).id;
                        break;
                    case eVehicleTargetTypes.SERVICE:
                        targetId = _SQL.services.Single(u => u.email == email).id;
                        break;
                }

                Token t;

                if(_SQL.tokens.Any(tt=>tt.target_type == targetType && tt.target_id == (int)targetId && tt.token_type == eTokenType.ASK_NEW_PASSWORD))
                {
                    t = _SQL.tokens.Single(tt => tt.target_type == targetType && tt.target_id == (int)targetId && tt.token_type == eTokenType.ASK_NEW_PASSWORD);
                    if(t.expire <= DateTime.Now.AddHours(1)) //Lejárt, vagy 1 órán belül lejár
                    {
                        t.expire = DateTime.Now.AddHours(1);
                        this._SQL.tokens.Update(t);
                    }
                }
                else
                {
                    t = new Token()
                    {
                        target_id = targetId,
                        target_type = targetType,
                        token_type = eTokenType.ASK_NEW_PASSWORD,
                        available = true,
                        expire = DateTime.Now.AddDays(1),
                        token = Functions.ReplaceSpecials(Convert.ToBase64String(Guid.NewGuid().ToByteArray())) + "t" + (int)targetType + "i" +targetId
                    };

                    this._SQL.tokens.Add(t);
                }

                this._SQL.SaveChanges();

                _Notification.AddSuccessToastMessage("Az új jelszó megadásához szükséges információkat elküldtük az email címére!");
                MailSender.SendNewPasswordMail(email, t, this.Request.Host.ToString());
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPasswordModel password)
        {
            if(loginType != eVehicleTargetTypes.NONE)
            {
                _Notification.AddErrorToastMessage("A felhasználó be van jelentkezve!");
                return Redirect("/Home/");
            }
            if (string.IsNullOrEmpty(password.newPassword) || string.IsNullOrEmpty(password.newPasswordRepeat))
            {
                this._Notification.AddErrorToastMessage("Minden mezőt megfelelően ki kell tölteni!");
                return BadRequest("Minden mezőt megfelelően ki kell tölteni!");
            }
            else if (password.newPassword != password.newPasswordRepeat)
            {
                this._Notification.AddErrorToastMessage("A jelszavak nem egyeznek!");
                return BadRequest("A jelszavak nem egyeznek!");
            }
            else if (password.newPassword.Length < 6)
            {
                this._Notification.AddErrorToastMessage("A jelszavak hossza min. 6 karakter!");
                return BadRequest("A jelszavak hossza min. 6 karakter!");
            }
            else
            {
                Token t = this._SQL.tokens.SingleOrDefault(tt => tt.token == password.token.ToString() && tt.token_type == eTokenType.ASK_NEW_PASSWORD);
                if(t == null)
                {
                    this._Notification.AddErrorToastMessage("A token érvénytelen!");
                    return BadRequest("A token érvénytelen!");
                }

                if(t.expire <= DateTime.Now)
                {
                    this._Notification.AddErrorToastMessage("A token lejárt!");
                    return BadRequest("A token lejárt!");
                }

                switch (password.userType)
                {
                    case eVehicleTargetTypes.DEALER:
                        Dealer d = _SQL.dealers.SingleOrDefault(de => de.id == password.userId);
                        if (d != null)
                        {
                            d.password = PasswordManager.GenerateHash(password.newPassword);
                            _SQL.dealers.Update(d);
                            _SQL.tokens.Remove(t);
                            _SQL.SaveChanges();
                            _Notification.AddSuccessToastMessage("Sikeres jelszó módosítás!");
                            return Ok();
                        }
                        else
                        {
                            this._Notification.AddErrorToastMessage("A keresett szerviz nem érhető el!");
                            return BadRequest("A keresett szerviz nem érhető el!");
                        }
                        break;
                    case eVehicleTargetTypes.USER:
                        User u = _SQL.users.SingleOrDefault(usr => usr.id == password.userId);
                        if (u != null)
                        {
                            u.password = PasswordManager.GenerateHash(password.newPassword);
                            _SQL.users.Update(u);
                            _SQL.tokens.Remove(t);
                            _SQL.SaveChanges();
                            _Notification.AddSuccessToastMessage("Sikeres jelszó módosítás!");
                            return Ok();
                        }
                        else
                        {
                            this._Notification.AddErrorToastMessage("A keresett szerviz nem érhető el!");
                            return BadRequest("A keresett szerviz nem érhető el!");
                        }
                        break;

                    case eVehicleTargetTypes.SERVICE:
                        Service s = _SQL.services.SingleOrDefault(se => se.id == password.userId);
                        if(s != null)
                        {
                            s.password = PasswordManager.GenerateHash(password.newPassword);
                            _SQL.services.Update(s);
                            _SQL.tokens.Remove(t);
                            _SQL.SaveChanges();
                            _Notification.AddSuccessToastMessage("Sikeres jelszó módosítás!");
                            return Ok();
                        }
                        else
                        {
                            this._Notification.AddErrorToastMessage("A keresett szerviz nem érhető el!");
                            return BadRequest("A keresett szerviz nem érhető el!");
                        }
                        break;

                    case eVehicleTargetTypes.FACTORY: //Bár ilyen nem is történhetne?!
                        this._Notification.AddErrorToastMessage("Tiltott gyár művelet!");
                        return BadRequest("Tiltott gyár művelet!");
                        break;
                }
                _Notification.AddErrorToastMessage("Hiba lépett fel!");
                return BadRequest("Hiba lépett fel!");
            }
        }
        #endregion
    }
}

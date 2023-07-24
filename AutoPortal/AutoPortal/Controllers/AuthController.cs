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
            await _SQL.SaveChangesAsync();

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

                if (m.regType) { //Szerviz
                    _SQL.services.Add(new Service()
                    {
                        email = m.email,
                        description= m.description,
                        name = m.name,
                        password= PasswordManager.GenerateHash(m.password),
                        phone = m.phone,
                        status = eAccountStatus.None
                    });
                } else { //Kereskedő
                    _SQL.dealers.Add(new Dealer()
                    {
                        email = m.email,
                        description = m.description,
                        name = m.name,
                        password = PasswordManager.GenerateHash(m.password),
                        phone = m.phone,
                        status = eAccountStatus.None
                    });
                }

                await _SQL.SaveChangesAsync();

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
                    /*
                    if (!user.status.HasFlag(eAccountStatus.EMAIL_CONFIRM)) {
                        this._Notification.AddErrorToastMessage("A felhasználó nem elérhető!");
                        return View();
                    }*/

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
    }
}

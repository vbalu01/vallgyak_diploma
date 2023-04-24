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
                enabled = true
            };
            _SQL.users.Add(u);
            await _SQL.SaveChangesAsync();

            _Notification.AddSuccessToastMessage("Sikeres regisztráció!");
            resp.Message = "Register success!";
            return Ok(resp.ToString());
        }
        #endregion

        #region Login

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if(!_SQL.users.Any(u=>u.email == email)) { 
                _Notification.AddInfoToastMessage("Hibás felhasználónév, vagy jelszó!");
                return View();
            }

            User user = await this._SQL.users.SingleAsync(u => u.email == email);

            if (PasswordManager.AreEqual(password, user.password)) {
                if (!user.enabled) {
                    this._Notification.AddErrorToastMessage("A felhasználó nem elérhető!");
                    return View();
				}

                ClaimsPrincipal princ = this.GenerateClaim(user);
                await this.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princ, new AuthenticationProperties()
                {
                    IsPersistent = true
                });
                this._Notification.AddSuccessToastMessage("A bejelentkezés sikeres!");
                return Redirect("/Home");
            }
            else {
                this._Notification.AddInfoToastMessage("Hibás felhasználónév, vagy jelszó!");
                return View();
            }
        }

        private ClaimsPrincipal GenerateClaim(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.email),
                new Claim("UserId", user.id.ToString())
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

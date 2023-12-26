using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Net;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace AutoPortal.Controllers
{
    public abstract class BaseController : Controller
    {
        protected SQL _SQL { get; private set; }
        protected IToastNotification _Notification { get; private set; }
        protected IWebHostEnvironment _Environment;
        protected IConfiguration _Configuration { get; private set; }
        protected JsonResponse resp;
        protected int loginId { get; private set; }
        protected eVehicleTargetTypes loginType { get; private set; }
        

        protected dynamic user;

        public BaseController(IConfiguration configuration, SQL sql, IToastNotification notification)
        {
            this._Configuration = configuration;
            this._SQL = sql;
            this._Notification = notification;
            this.resp = new JsonResponse();
            resp.Success = true;
        }

        public BaseController(IConfiguration configuration, SQL sql, IToastNotification notification, IWebHostEnvironment environment)
        {
            this._Configuration = configuration;
            this._SQL = sql;
            this._Notification = notification;
            this.resp = new JsonResponse();
            resp.Success = true;
            _Environment = environment;
        }

        
        public override void OnActionExecuting(ActionExecutingContext context) //Ez a metódus minden API hívás előtt lefut
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var methodInfo = descriptor.MethodInfo;

            if (methodInfo.GetCustomAttributes(typeof(ActionLog), false).Any())
            {
                ActionLog attr = (ActionLog)methodInfo.GetCustomAttributes(typeof(ActionLog), true).First(atr => atr.GetType() == typeof(ActionLog));
                if (attr.LogParams)
                    WriteLog("ActionExecute: + " + context.ActionDescriptor.DisplayName + "\n" + JsonConvert.SerializeObject(context.ActionArguments));
                else
                    WriteLog("ActionExecute: + " + context.ActionDescriptor.DisplayName);
            }

            if (this.HttpContext.User.Claims.Any(c => c.Type == "UserId")) //Bejelentkezett felhasználó
            {
                this.loginId = Convert.ToInt32(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "UserId").Value);

                switch(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "LoginType").Value)
                {
                    case "USER":
                        user = this._SQL.users.SingleOrDefault(u => u.id == this.loginId);
                        loginType = eVehicleTargetTypes.USER;
                        if (((User)user).status.HasFlag(eAccountStatus.BANNED))
                        {
                            this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            Redirect("/Auth/Login");
                        }
                        break;
                    case "SERVICE":
                        user = this._SQL.services.SingleOrDefault(u => u.id == this.loginId);
                        loginType = eVehicleTargetTypes.SERVICE;
                        if (((Service)user).status.HasFlag(eAccountStatus.BANNED))
                        {
                            this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            Redirect("/Auth/Login");
                        }
                        break;
                    case "DEALER":
                        user = this._SQL.dealers.SingleOrDefault(u => u.id == this.loginId);
                        loginType = eVehicleTargetTypes.DEALER;
                        if (((Dealer)user).status.HasFlag(eAccountStatus.BANNED))
                        {
                            this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            Redirect("/Auth/Login");
                        }
                    break;
                }

            }
            else
            {
                this.loginType = eVehicleTargetTypes.NONE;
            }
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)//Ez a metódus lefut minden API hívás lefutása után
        {
            base.OnActionExecuted(context);
        }

        protected void WriteLog(string text, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
        {
            Log.LogMessageAsync(text, this.HttpContext, caller, file);
        }
    }
}

using AutoPortal.Libs;
using AutoPortal.Models;
using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Net;
using System.Runtime.CompilerServices;

namespace AutoPortal.Controllers
{
    public abstract class BaseController : Controller
    {
        protected SQL _SQL { get; private set; }
        protected IToastNotification _Notification { get; private set; }
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

        public override void OnActionExecuting(ActionExecutingContext context) //Ez a metódus minden API hívás előtt lefut
        {
            if (this.HttpContext.User.Claims.Any(c => c.Type == "UserId")) //Bejelentkezett felhasználó
            {
                this.loginId = Convert.ToInt32(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "UserId").Value);
                switch(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "LoginType").Value)
                {
                    case "USER":
                        user = this._SQL.users.SingleOrDefault(u => u.id == this.loginId);
                        loginType = eVehicleTargetTypes.USER;
                        break;
                    case "SERVICE":
                        user = this._SQL.services.SingleOrDefault(u => u.id == this.loginId);
                        loginType = eVehicleTargetTypes.SERVICE;
                        break;
                    case "DEALER":
                        user = this._SQL.dealers.SingleOrDefault(u => u.id == this.loginId);
                        loginType = eVehicleTargetTypes.DEALER;
                    break;
                }
            }
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)//Ez a metódus lefut minden API hívás lefutása után
        {
            base.OnActionExecuted(context);
        }
    }
}

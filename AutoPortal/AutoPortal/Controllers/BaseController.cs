using AutoPortal.Libs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System.Runtime.CompilerServices;

namespace AutoPortal.Controllers
{
    public abstract class BaseController : Controller
    {
        private SQL sql;

        protected SQL _SQL { get; private set; }
        protected IToastNotification _Notification { get; private set; }
        protected IConfiguration _Configuration { get; private set; }

        //protected User user;

        public BaseController(IConfiguration configuration, SQL sql, IToastNotification notification)
        {
            this._Configuration = configuration;
            this._SQL = sql;
            this._Notification = notification;
        }

        protected BaseController(SQL sql)
        {
            this.sql = sql;
        }

        public override void OnActionExecuting(ActionExecutingContext context) //Ez a metódus minden API hívás előtt lefut
        {
            if (this.HttpContext.User.Claims.Any(c => c.Type == "UserId")) //Bejelentkezett felhasználó
            {
                int uid = Convert.ToInt32(this.HttpContext.User.Claims.SingleOrDefault(c => c.Type == "UserId").Value);
                //user = this._SQL.Users.SingleOrDefault(u => u.ID == uid);
            }
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)//Ez a metódus lefut minden API hívás lefutása után
        {
            base.OnActionExecuted(context);
        }
    }
}

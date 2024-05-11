using AutoPortal.Libs;
using AutoPortal.Models.AppModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NToastNotify;
using System.Security.Claims;
using System.Text;

namespace AutoPortal
{
    public class Startup
    {
        public static ServerVersion v_MysqlVersion;
        public static string SQLConnectionString { get; set; }
        public static string TokenKey { get; set; }
        private static Log Log;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            SQLConnectionString = Configuration.GetConnectionString("SQL");
            TokenKey = Configuration.GetValue(typeof(string), "TokenKey").ToString();
            v_MysqlVersion = new MySqlServerVersion(new Version(8, 0, 26));
            Log = new Log();
            services.AddDbContext<SQL>(option =>
            {
                option.UseMySql(SQLConnectionString, v_MysqlVersion);
            });

            //Mail küldő adatok
            MailSettingsModel.host = Configuration.GetSection("MailSettings").GetValue(typeof(string), "Host").ToString();
            MailSettingsModel.port = Convert.ToInt32(Configuration.GetSection("MailSettings").GetValue(typeof(int), "Port"));
            MailSettingsModel.username = Configuration.GetSection("MailSettings").GetValue(typeof(string), "Username").ToString();
            MailSettingsModel.password = Configuration.GetSection("MailSettings").GetValue(typeof(string), "Password").ToString();
            MailSettingsModel.useSSL = Convert.ToBoolean(Configuration.GetSection("MailSettings").GetValue(typeof(bool), "MailUseSSL"));
            MailSettingsModel.defaultSender = Configuration.GetSection("MailSettings").GetValue(typeof(string), "DefaultSender").ToString();

            #region Authorization
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(x =>
            {
                x.LoginPath = "/Auth/Login";
                x.LogoutPath = "/Auth/Logout";
                x.LogoutPath = "/Auth/Register";
                x.AccessDeniedPath = "/Auth/AccessDenied";
            }).AddJwtBearer(jwt =>
            {
                jwt.RequireHttpsMetadata = false;
                jwt.SaveToken = true;
                jwt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Startup.TokenKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            }); ;
            services.AddAuthorization(a => a.AddPolicy("User", policy => policy.RequireClaim(ClaimTypes.Role, "User")));
            services.AddAuthorization(a => a.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin")));

            #endregion Authorization
            services.AddMvc().AddNToastNotifyToastr(null, new NToastNotifyOption { DefaultAlertTitle = "Értesítés", DefaultErrorTitle = "Hiba", DefaultInfoTitle = "Információ", DefaultSuccessTitle = "Siker", DefaultWarningTitle="Figyelmeztetés"});
            services.AddControllersWithViews();


            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseNToastNotify();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

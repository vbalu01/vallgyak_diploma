using AutoPortal.Libs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

        // This method gets called by the runtime. Use this method to add services to the container.
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
            });
            services.AddAuthorization(a => a.AddPolicy("User", policy => policy.RequireClaim(ClaimTypes.Role, "User")));
            services.AddAuthorization(a => a.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin")));

            #endregion Authorization
            services.AddMvc().AddNToastNotifyToastr();
            services.AddControllersWithViews();


            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
